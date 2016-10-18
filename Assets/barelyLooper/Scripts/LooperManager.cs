using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class that manages the recording and playback of the loopers.
public class LooperManager : MonoBehaviour {
  // Prefab object to instantiate loopers.
  public GameObject looperPrefab;

  // Prefab object to instantiate a record indicator object.
  public GameObject recorderPrefab;

  // Mic recorder.
  public Recorder recorder;

  // Reticle to handle gaze based user input.
  public GvrReticle reticle;

  // Command manager.
  public CommandManager commandManager;

  // Looper instances.
  private List<LoopController> loopers;

  // Root game object to store the looper instances.
  private GameObject loopersRoot;

  // Currently selected looper.
  private LoopController currentLooper;

  // Record indicator.
  private RecordController recordVisualizer;

  // Playback length in seconds.
  private double playbackLength;

  // Is recording fixed length?
  private bool fixedLength;

  // Is currently playing?
  private bool isPlaying;

  // Should traced path recorded with the loop?
  public bool recordPath;

  void Awake () {
    loopers = new List<LoopController>();
    currentLooper = null;
    recordVisualizer = GameObject.Instantiate(recorderPrefab).GetComponent<RecordController>();
    recordVisualizer.Deactivate();
    commandManager = new CommandManager();

    Reset();

    isPlaying = true;
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
    looper.transform.parent = loopersRoot.transform;
    looper.GetComponent<Renderer>().enabled = false;
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

  // Clears the scene.
  public void Reset () {
    loopers.Clear();
    if (loopersRoot != null) {
      Destroy(loopersRoot);
    }
    loopersRoot = new GameObject("Loopers");
    playbackLength = 0.0;
  }

  // Pauses the playback.
  public void Pause () {
    for (int i = 0; i < loopers.Count; ++i) {
      loopers[i].PausePlayback();
    }
    isPlaying = false;
  }

  // Resumes the playback.
  public void UnPause () {
    for (int i = 0; i < loopers.Count; ++i) {
      loopers[i].UnPausePlayback();
    }
    isPlaying = true;
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
    if (!isPlaying) {
      // Skip processing when paused.
      return;
    }
    if (targetObject == null && !recorder.IsRecording) {
      // Start recording.
      recorder.StartRecording();
      recordVisualizer.SetTransform(Camera.main.transform);
      recordVisualizer.Activate();
      currentLooper = CreateLooper(Camera.main.transform);
      if (recordPath) {
        currentLooper.pathRecorder.StartRecording(recordVisualizer.transform, 
                                                  AudioSettings.dspTime);
      }
    }
  }

  // Implements |GvrReticle.OnGazePointerUp| callback.
  private void OnGazePointerUp (GameObject targetObject) {
    if (!isPlaying) {
      // Skip processing when paused.
      return;
    }
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
    currentLooper.StartPlayback(dspTime, playbackOffsetSamples);
    // Set the loop path.
    if (recordPath) {
      currentLooper.pathRecorder.StopRecording(startTime + length);
      currentLooper.pathRecorder.path.AddKey((float)(startTime + loopLength),
                                             currentLooper.pathRecorder.path.GetKey(0));
    }
    currentLooper.GetComponent<Renderer>().enabled = true;
  }
}
