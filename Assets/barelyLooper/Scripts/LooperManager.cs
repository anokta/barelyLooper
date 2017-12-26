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
  public MicRecorder micRecorder;

  // Path recorder.
  public PathRecorder pathRecorder;

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

  // Input/output latency in seconds.
  private double outputLatency;

  // Playback length in samples.
  private int playbackLengthSamples;

  // Playback length in seconds.
  private double playbackStartTime;

  // Record absolute start time in seconds.
  private double recordStartTime;

  // Record absolute end time in seconds.
  private double recordEndTime;

  // Is recording fixed length?
  private bool fixedLength;

  // Is currently playing?
  private bool isPlaying;

  // Is currently recording?
  private bool isRecording;

  // Should traced path recorded with the loop?
  public bool recordPath;

  void Awake() {
    loopers = new List<LoopController>();
    currentLooper = null;
    recordVisualizer = GameObject.Instantiate(recorderPrefab).GetComponent<RecordController>();
    recordVisualizer.Deactivate();
    commandManager = new CommandManager();
    // Get output latency in samples.
    int bufferLength = 0, numBuffers = 0;
    AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
    outputLatency = (double) numBuffers * bufferLength / AudioSettings.outputSampleRate;

    Reset();

    isPlaying = true;
    isRecording = false;
    fixedLength = true;
    recordPath = false;
  }

  void OnEnable() {
    reticle.OnGazePointerDown = OnGazePointerDown;
    reticle.OnGazePointerUp = OnGazePointerUp;
  }

  void OnDisable() {
    reticle.OnGazePointerDown = null;
    reticle.OnGazePointerUp = null;
  }

  void OnApplicationPause(bool pauseStatus) {
    if (pauseStatus) {
      Pause();
    } else {
      UnPause();
    }
  }

  void Update() {
    if (recordPath && isRecording) {
      recordVisualizer.SetTransform(Camera.main.transform);
    }
  }

  // Creates a new looper with respect to the |camera|.
  public LoopController CreateLooper(Transform camera) {
    LoopController looper = GameObject.Instantiate(looperPrefab).GetComponent<LoopController>();
    looper.transform.parent = loopersRoot.transform;
    looper.GetComponent<Renderer>().enabled = false;
    looper.looperManager = this;
    looper.SetTransform(camera, Vector3.zero);
    loopers.Add(looper);
    return looper;
  }

  // Destroys the game object of given |looper|.
  public void DestroyLooper(LoopController looper) {
    loopers.Remove(looper);
    GameObject.Destroy(looper.gameObject);
  }

  // Clears the scene.
  public void Reset() {
    loopers.Clear();
    if (loopersRoot != null) {
      Destroy(loopersRoot);
    }
    loopersRoot = new GameObject("Loopers");
    playbackLengthSamples = 0;
  }

  // Pauses the playback.
  public void Pause() {
    for (int i = 0; i < loopers.Count; ++i) {
      loopers[i].PausePlayback();
    }
    isPlaying = false;
  }

  // Resumes the playback.
  public void UnPause() {
    for (int i = 0; i < loopers.Count; ++i) {
      loopers[i].UnPausePlayback();
    }
    isPlaying = true;
  }

  // Toggles fixed length recording.
  public void ToggleFixedLength() {
    fixedLength = !fixedLength;
  }

  public void ToggleRecordPath() {
    recordPath = !recordPath;
  }

  public void DoubleLength() {
    playbackLengthSamples *= 2;
  }

  public void HalveLength() {
    playbackLengthSamples /= 2;
  }

  // Implements |GvrReticle.OnGazePointerDown| callback.
  private void OnGazePointerDown(GameObject targetObject) {
    if (!isPlaying) {
      // Skip processing when paused.
      return;
    }
    if (targetObject == null && !isRecording) {
      // Start mic recording.
      isRecording = true;
      recordStartTime = AudioSettings.dspTime;
      recordVisualizer.SetTransform(Camera.main.transform);
      recordVisualizer.Activate();
      currentLooper = CreateLooper(Camera.main.transform);
      // Start path recording.
      pathRecorder.StartRecording(AudioSettings.dspTime, micRecorder.Frequency, 
                                  recordVisualizer.transform);
    }
  }

  // Implements |GvrReticle.OnGazePointerUp| callback.
  private void OnGazePointerUp(GameObject targetObject) {
    if (!isPlaying) {
      // Skip processing when paused.
      return;
    }
    if (isRecording) {
      // Stop recording.
      isRecording = false;
      recordEndTime = AudioSettings.dspTime;
      float[] recordData = null;
      int latencySamples = micRecorder.GetRecordedData(recordStartTime, recordEndTime, 
                                                       out recordData);
      SetLooperData(recordData, latencySamples);      
      recordVisualizer.Deactivate();
    }
  }

  private void SetLooperData(float[] data, int latencySamples) {
    int lengthSamples = (int) ((recordEndTime - recordStartTime) * micRecorder.Frequency);
    if (loopers.Count == 1) {
      playbackLengthSamples = lengthSamples;
      playbackStartTime = recordStartTime;
    }
    int loopLengthSamples = 
      (fixedLength || lengthSamples < playbackLengthSamples) ? playbackLengthSamples : 
      Mathf.RoundToInt((float) lengthSamples / playbackLengthSamples) * playbackLengthSamples;
    // Set the audio clip.
    int offsetSamples = 
      ((int) ((recordStartTime - playbackStartTime) * micRecorder.Frequency)) % playbackLengthSamples;
    currentLooper.SetAudioClip(data, loopLengthSamples, offsetSamples, micRecorder.Frequency, 
                               latencySamples);
    // Start the playback.
    double dspTime = AudioSettings.dspTime; 
    int playbackOffsetSamples = 
      (int) ((dspTime - playbackStartTime + outputLatency) * micRecorder.Frequency);
    currentLooper.StartPlayback(dspTime, playbackOffsetSamples);
    // Set the loop path.
    LooperPath path = pathRecorder.StopRecording(recordEndTime);
    currentLooper.SetPath(path, loopLengthSamples, offsetSamples, micRecorder.Frequency);

    currentLooper.GetComponent<Renderer>().enabled = true;
  }
}
