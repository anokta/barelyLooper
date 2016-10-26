using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Class that controls a looper object.
public class LoopController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
  // Audio source.
  public GvrAudioSource source;

  // Looper manager instance.
  public LooperManager looperManager;

  // Recorded looper path to be traced.
  public PathRecorder pathRecorder;

  // Loop data in samples.
  private float[] data;

  // Loop length in samples.
  private int lengthSamples;

  // Object distance to camera.
  private float distance;

  // Hit transform position.
  private Vector3 pressPosition;

  // Hit transform rotation.
  private Quaternion pressRotation;

  // Hit look direction.
  private Vector3 pressDirection;

  // Hit offset from the center point.
  private Vector3 pressOffset;

  // Hit time in seconds.
  private float pressTime;

  // Playback state when paused.
  private double pauseTime, pauseOffset;
 
  // Allowed maximum click angle in degrees.
  private static float clickAngleThreshold = 2.0f;

  // Allowed maximum click time in seconds.
  private static float clickTimeThreshold = 0.35f;

  void Awake () {
    distance = transform.position.z;
    pauseOffset = 0.0;
  }

  void Update () { 
    if (pathRecorder.path != null && source.isPlaying && !pathRecorder.isRecording) {
      transform.position = pathRecorder.path.Evaluate((float)(AudioSettings.dspTime - pauseOffset));
      Vector3 direction = transform.position - Camera.main.transform.position;
      transform.rotation = Quaternion.LookRotation(-direction);
    }
  }

  // Sets the transform of the looper to directly face to the |camera| with given |offset|.
  public void SetTransform (Transform camera, Vector3 offset) {
    Vector3 direction = distance * camera.forward + camera.rotation * offset;
    transform.position = camera.position + direction;
    transform.rotation = Quaternion.LookRotation(-direction,
                                                 Vector3.Cross(camera.right, direction));
  }

  public void SetAudioClip (float[] originalData, int loopLengthSamples, int offsetSamples, 
                            int frequency, int fadeSamples) {
    lengthSamples = loopLengthSamples;
    data = new float[lengthSamples];
    // Fill in the loop data.
    fadeSamples = Mathf.Min(fadeSamples, lengthSamples);
    int crossfadeSamples = 2 * fadeSamples;
    int startPosition = 
      Mathf.Max(originalData.Length - lengthSamples + fadeSamples, crossfadeSamples);
    int targetStartPosition = (startPosition - crossfadeSamples + offsetSamples) % lengthSamples; 
    for(int i = 0; i < originalData.Length - startPosition; ++i) {
      data[(targetStartPosition + i) % lengthSamples] = originalData[startPosition + i];
    }
    // Crossfade the end by Hann window to loop seamlessly.
    int leftPosition = startPosition - crossfadeSamples;
    int rightPosition = startPosition - fadeSamples;
    targetStartPosition += lengthSamples - fadeSamples;
    for(int i = 0; i < Mathf.Min(lengthSamples, fadeSamples); ++i) {
      float fade = 0.5f * (1.0f + Mathf.Cos(Mathf.PI * i / fadeSamples));
      data[(targetStartPosition + i) % lengthSamples] = 
        fade * originalData[leftPosition + i] + (1.0f - fade) * originalData[rightPosition + i];
    }
    // Set loop data as the new clip.
    source.clip = AudioClip.Create("Loop", lengthSamples, 1, frequency, false);
    source.clip.SetData(data, 0);
  }

  // Starts loop playback at given dsp |startTime| with |playbackOffsetSamples| offset.
  public void StartPlayback (double startTime, int playbackOffsetSamples) {
    source.timeSamples = playbackOffsetSamples % source.clip.samples;
    source.PlayScheduled(startTime);
  }

  // Pauses loop playback.
  public void PausePlayback () {
    if (source.isPlaying) {
      pauseTime = AudioSettings.dspTime;
      source.Pause();
    }
  }

  // Un-pauses loop playback.
  public void UnPausePlayback () {
    if (!source.isPlaying) {
      pauseOffset += AudioSettings.dspTime - pauseTime;
      source.UnPause();
    }
  }

  // Implements |IPointerDownHandler.OnPointerDown| callback.
  public void OnPointerDown (PointerEventData eventData) {
    if (source.isPlaying && !looperManager.recordPath) {
      pathRecorder.path = null;
    }

    pressTime = Time.time;
    Transform camera = eventData.pressEventCamera.transform;
    pressDirection = camera.forward;
    pressPosition = transform.position;
    pressRotation = transform.rotation;
    // Calculate where the trigger was pressed relative to the center of the looper object.
    Vector3 rotatedOffset = (transform.position - camera.position) - distance * camera.forward;
    pressOffset = Quaternion.Inverse(camera.rotation) * rotatedOffset;
  }

  // Implements |IPointerUpHandler.OnPointerUp| callback.
  public void OnPointerUp (PointerEventData eventData) {
    if (looperManager.recordPath && pathRecorder.isRecording) {
      double lengthSeconds = (double)lengthSamples / source.clip.frequency;
      pathRecorder.StopRecording(AudioSettings.dspTime);
      pathRecorder.path.AddKey((float)(pathRecorder.recordStartTime + lengthSeconds),
                               pathRecorder.path.GetKey(0));
    } 

    Transform camera = eventData.pressEventCamera.transform;
    if (Time.time < pressTime + clickTimeThreshold &&
        Vector3.Angle(camera.forward, pressDirection) < clickAngleThreshold) {
      // Remove the looper.
      looperManager.DestroyLooper(this);
    } else { 
      // Add move command to the manager.
      looperManager.commandManager.ExecuteCommand(new MoveCommand(this,
                                                                  pressPosition,
                                                                  pressRotation,
                                                                  transform.position,
                                                                  transform.rotation));
    }
  }

  // Implements |IDragHandler.OnDrag| callback.
  public void OnDrag (PointerEventData eventData) {
    if (!source.isPlaying) {
      return;
    }

    // Drag and drop the looper object.
    SetTransform(eventData.pressEventCamera.transform, pressOffset);

    if (looperManager.recordPath && !pathRecorder.isRecording) {
      pathRecorder.StartRecording(transform, AudioSettings.dspTime);
      pauseOffset = 0.0;
    } 
  }
}
