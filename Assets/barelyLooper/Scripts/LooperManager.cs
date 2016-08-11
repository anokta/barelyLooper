using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class that manages the recording and playback of the loopers.
public class LooperManager : MonoBehaviour {
  // Prefab object to instantiate loopers.
  public GameObject looperPrefab;

  // Mic recorder.
  public Recorder recorder;

  // Reticle to handle gaze based user input.
  public GvrReticle reticle;

  // Looper instances.
  private List<Looper> loopers;

  // Currently selected looper.
  private Looper currentLooper;

  // Playback length in seconds.
  private double playbackLength;

  void Awake () {
    loopers = new List<Looper>();
    currentLooper = null;

    playbackLength = 0.0;
  }

  void OnEnable () {
    recorder.OnFinishRecord = OnFinishRecord;
    reticle.OnGazePointerDown = OnGazePointerDown;
    reticle.OnGazePointerUp = OnGazePointerUp;
  }

  void OnDisable () {
    recorder.OnFinishRecord = null;
    reticle.OnGazePointerDown = null;
    reticle.OnGazePointerUp = null;
  }

  // Creates a new looper with respect to the |camera|.
  public Looper CreateLooper (Transform camera) {
    Looper looper = GameObject.Instantiate(looperPrefab).GetComponent<Looper>();
    looper.looperManager = this;
    looper.SetTransform(Camera.main.transform, Vector3.zero);
    loopers.Add(looper);
    return looper;
  }

  // Destroys the game object of given |looper|.
  public void DestroyLooper (Looper looper) {
    loopers.Remove(looper);
    GameObject.Destroy(looper.gameObject);
  }

  // Implements |GvrReticle.OnGazePointerDown| callback.
  private void OnGazePointerDown (GameObject targetObject) {
    if (targetObject == null && !recorder.IsRecording) {
      // Start recording.
      recorder.StartRecording();
      currentLooper = CreateLooper(Camera.main.transform);
    }
  }

  // Implements |GvrReticle.OnGazePointerUp| callback.
  private void OnGazePointerUp (GameObject targetObject) {
    if (recorder.IsRecording) {
      // Stop recording.
      recorder.StopRecording();
    }
  }

  // Implements |Recorder.OnFinishRecord| callback.
  private void OnFinishRecord (double startTime, double length, int frequency, float[] data) {
    if (loopers.Count == 1) {  // playback hasn't started yet
      playbackLength = length;
    }
    // Set the audio clip.
    currentLooper.SetAudioClip(data, playbackLength, frequency);
    // Start the playback.
    double dspTime = AudioSettings.dspTime;
    int playbackOffsetSamples = (int)(frequency * (dspTime - (startTime - recorder.RecordLatency)));
    currentLooper.StartPlayback(dspTime, playbackOffsetSamples);
  }
}
