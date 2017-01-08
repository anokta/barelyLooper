using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Class that controls a looper object.
public class LoopController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
  // Audio source.
  public GvrAudioSource source;

  // Looper manager instance.
  public LooperManager looperManager;

  // Loop data in samples.
  private float[] data;

  // Path to be traced.
  public Path path;

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
 
  // Allowed maximum click angle in degrees.
  private static float clickAngleThreshold = 2.0f;

  // Allowed maximum click time in seconds.
  private static float clickTimeThreshold = 0.35f;

  // TODO(anokta): Temp to be refactored - move it back to looper manager.
  private double pathStartTime, pathEndTime;
  private int pathOffsetSamples;

  void Awake () {
    distance = transform.position.z;
    path = null;
  }

  void Update () { 
    if (path != null) {
      transform.position = 
        SphericalToCartesian(path.Evaluate(source.timeSamples)) + Camera.main.transform.position;
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

  public void SetAudioClip (float[] originalData, int lengthSamples, int offsetSamples, 
                            int frequency, int fadeSamples) {
    data = new float[lengthSamples];
    // Fill in the loop data.
    fadeSamples = Mathf.Min(fadeSamples, lengthSamples);
    int crossfadeSamples = 2 * fadeSamples;
    int startPosition = 
      Mathf.Max(originalData.Length - lengthSamples + fadeSamples, crossfadeSamples);
    int targetStartPosition = (startPosition - crossfadeSamples + offsetSamples) % lengthSamples; 
    for (int i = 0; i < originalData.Length - startPosition; ++i) {
      data[(targetStartPosition + i) % lengthSamples] = originalData[startPosition + i];
    }
    // Crossfade the end by Hann window to loop seamlessly.
    int leftPosition = startPosition - crossfadeSamples;
    int rightPosition = startPosition - fadeSamples;
    targetStartPosition += lengthSamples - fadeSamples;
    for (int i = 0; i < Mathf.Min(lengthSamples, fadeSamples); ++i) {
      float fade = 0.5f * (1.0f + Mathf.Cos(Mathf.PI * i / fadeSamples));
      data[(targetStartPosition + i) % lengthSamples] = 
        fade * originalData[leftPosition + i] + (1.0f - fade) * originalData[rightPosition + i];
    }
    // Set loop data as the new clip.
    source.clip = AudioClip.Create("Loop", lengthSamples, 1, frequency, false);
    source.clip.SetData(data, 0);
  }

  public void SetPath (Path originalPath, int lengthSamples, int offsetSamples, int frequency) {
    path = new Path();

    int endTimeSamples = originalPath.GetTime(originalPath.NumKeys - 1);
    int startTimeSamples = Mathf.Max(endTimeSamples - lengthSamples, 0);

    for (int i = 0; i < originalPath.NumKeys; ++i) {
      int currentTimeSamples = originalPath.GetTime(i);
      if (currentTimeSamples >= startTimeSamples && currentTimeSamples != endTimeSamples) {
        path.AddKey((currentTimeSamples + offsetSamples) % lengthSamples, 
                    CartesianToSpherical(originalPath.GetKey(i) - Camera.main.transform.position));
      }
    }
    path.SetLengthSamples(lengthSamples);
  }

  // Starts loop playback at given dsp |startTime| with |playbackOffsetSamples| offset.
  public void StartPlayback (double startTime, int playbackOffsetSamples) {
    source.timeSamples = playbackOffsetSamples % source.clip.samples;
    source.PlayScheduled(startTime);
  }

  // Pauses loop playback.
  public void PausePlayback () {
    if (source.isPlaying) {
      source.Pause();
    }
  }

  // Un-pauses loop playback.
  public void UnPausePlayback () {
    if (!source.isPlaying) {
      source.UnPause();
    }
  }

  // Implements |IPointerDownHandler.OnPointerDown| callback.
  public void OnPointerDown (PointerEventData eventData) {
    pressTime = Time.time;
    Transform camera = eventData.pressEventCamera.transform;
    pressDirection = camera.forward;
    pressPosition = transform.position;
    pressRotation = transform.rotation;
    // Calculate where the trigger was pressed relative to the center of the looper object.
    Vector3 rotatedOffset = (transform.position - camera.position) - distance * camera.forward;
    pressOffset = Quaternion.Inverse(camera.rotation) * rotatedOffset;

    if (looperManager.recordPath) {
      pathStartTime = AudioSettings.dspTime;
      pathOffsetSamples = source.timeSamples;
      path = null;
      looperManager.pathRecorder.StartRecording(pathStartTime, source.clip.frequency, transform);
    }
  }

  // Implements |IPointerUpHandler.OnPointerUp| callback.
  public void OnPointerUp (PointerEventData eventData) {
    Transform camera = eventData.pressEventCamera.transform;
    if (Time.time < pressTime + clickTimeThreshold &&
        Vector3.Angle(camera.forward, pressDirection) < clickAngleThreshold) {
      // Remove the looper.
      looperManager.DestroyLooper(this);
    } else { 
      if (looperManager.recordPath && path == null) {
        pathEndTime = AudioSettings.dspTime;
        Path recordPath = looperManager.pathRecorder.StopRecording(pathEndTime);
        SetPath(recordPath, source.clip.samples, pathOffsetSamples, source.clip.frequency);
      }
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
  }


  public static Vector3 SphericalToCartesian (Vector3 spherical) {
    Vector3 cartesian;
    float rCosElevation = spherical.x * Mathf.Cos(spherical.z);
    cartesian.x = rCosElevation * Mathf.Cos(spherical.y);
    cartesian.y = spherical.x * Mathf.Sin(spherical.z);
    cartesian.z = rCosElevation * Mathf.Sin(spherical.y);
    return cartesian;
  }

  public static Vector3 CartesianToSpherical (Vector3 cartesian) {
    Vector3 spherical;
    if (cartesian.x == 0.0f) {
      cartesian.x = Mathf.Epsilon;
    }
    spherical.x = cartesian.magnitude;
    spherical.y = Mathf.Atan(cartesian.z / cartesian.x);
    if (cartesian.x < 0.0f) {
      spherical.y += Mathf.PI;
    }
    spherical.z = Mathf.Asin(cartesian.y / spherical.x);
    return spherical;
  }
}
