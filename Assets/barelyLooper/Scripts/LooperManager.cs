using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class that manages the recording and playback of the loopers.
public class LooperManager : MonoBehaviour {
  // Prefab object to instantiate loopers.
  public GameObject looperPrefab;

  // Prefab object to instantiate a record indicator object.
  public GameObject recorderPrefab;

  // Path recorder.
  public PathRecorder pathRecorder;

  // Mic recorder.
  public Recorder recorder;

  // Reticle to handle gaze based user input.
  public GvrReticle reticle;

  // Looper instances.
  private List<LoopController> loopers;

  // Currently selected looper.
  private LoopController currentLooper;

  // Record indicator.
  private RecordController recordVisualizer;

  // Playback length in seconds.
  private double playbackLength;

  // Is recording fixed length?
  private bool fixedLength;

  // Should traced path recorded with the loop?
  private bool recordPath;

  void Awake () {
    loopers = new List<LoopController>();
    currentLooper = null;
    recordVisualizer = GameObject.Instantiate(recorderPrefab).GetComponent<RecordController>();
    recordVisualizer.Deactivate();

    playbackLength = 0.0;
    fixedLength = true;
    recordPath = false;
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

  void Update () {
    if (recordPath && recorder.IsRecording) {
      recordVisualizer.SetTransform(Camera.main.transform);
    }
  }

  // Creates a new looper with respect to the |camera|.
  public LoopController CreateLooper (Transform camera) {
    LoopController looper = GameObject.Instantiate(looperPrefab).GetComponent<LoopController>();
    looper.gameObject.SetActive(false);
    looper.looperManager = this;
    looper.SetTransform(camera, Vector3.zero);
    loopers.Add(looper);
    return looper;
  }

  // Destroys the game object of given |looper|.
  public void DestroyLooper (LoopController looper) {
    loopers.Remove(looper);
    GameObject.Destroy(looper.gameObject);
  }

  // Toggles fixed length recording.
  public void ToggleFixedLength () {
    fixedLength = !fixedLength;
  }

  public void ToggleRecordPath () {
    recordPath = !recordPath;
  }

  public void DoubleLength () {
    playbackLength *= 2.0f;
  }

  public void HalveLength () {
    playbackLength *= 0.5f;
  }

  // Implements |GvrReticle.OnGazePointerDown| callback.
  private void OnGazePointerDown (GameObject targetObject) {
    if (targetObject == null && !recorder.IsRecording) {
      // Start recording.
      recorder.StartRecording();
      recordVisualizer.SetTransform(Camera.main.transform);
      recordVisualizer.Activate();
      if (recordPath) {
        pathRecorder.StartRecording(recordVisualizer.transform, AudioSettings.dspTime);
      }
      currentLooper = CreateLooper(Camera.main.transform);
    }
  }

  // Implements |GvrReticle.OnGazePointerUp| callback.
  private void OnGazePointerUp (GameObject targetObject) {
    if (recorder.IsRecording) {
      // Stop recording.
      recorder.StopRecording();      
      recordVisualizer.Deactivate();
    }
  }

  // Implements |Recorder.OnFinishRecord| callback.
  private void OnFinishRecord (double startTime, double length, int frequency, float[] data) {
    if (loopers.Count == 1) {
      playbackLength = length;
    }
    double loopLength = (fixedLength || length < playbackLength) ?
      playbackLength : System.Math.Round(length / playbackLength) * playbackLength;
    // Set the audio clip.
    currentLooper.SetAudioClip(data, loopLength, frequency);
    // Start the playback.
    double dspTime = AudioSettings.dspTime;
    int playbackOffsetSamples = (int)(frequency * (dspTime - (startTime - recorder.RecordLatency)));
    currentLooper.gameObject.SetActive(true);
    currentLooper.StartPlayback(dspTime, playbackOffsetSamples);
    // Set the loop path.
    if (recordPath) {
      currentLooper.path = pathRecorder.StopRecording(startTime + length);
      currentLooper.path.AddKey((float)(startTime + loopLength), 
                                currentLooper.path.GetKey(currentLooper.path.Length - 1));
    }
  }
}
