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

  // Fade length in samples for each side of the loop.
  private int fadeLengthSamples;

  // Hit look direction.
  private Vector3 pressDirection;

  // Hit offset from the center point.
  private Vector3 pressOffset;

  // Hit time in seconds.
  private float pressTime;

  // Playback state when paused..
  private double pauseTime, pauseOffset;
 
  // Allowed maximum click angle in degrees.
  private static float clickAngleThreshold = 2.0f;

  // Allowed maximum click time in seconds.
  private static float clickTimeThreshold = 0.35f;

  void Awake () {
    distance = transform.position.z;
    fadeLengthSamples = AudioSettings.GetConfiguration().dspBufferSize;
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

  public void SetAudioClip (float[] originalData, double targetLength, int frequency) {
    // Fill in the loop data.
    lengthSamples = (int)(targetLength * frequency);
    data = new float[lengthSamples];
    for (int i = 0; i < originalData.Length; ++i) {
      data[i % lengthSamples] = originalData[i];
    }
    // Smoothen both ends.
    for (int i = 0; i < Mathf.Min(fadeLengthSamples, lengthSamples / 2); ++i) {
      float fade = 1.0f / Mathf.Sqrt(i + 1);
      data[i] *= fade;
      data[lengthSamples - i - 1] *= fade;
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
