using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LooperManager : MonoBehaviour {
  // Prefab object to instantiate loopers.
  public GameObject looperPrefab;

  // Recording color.
  public Color recordColor = Color.red;

  // Looper instances.
  private List<Looper> loopers;

  // Currently selected looper.
  private Looper currentLooper;

  // Mic recorder.
  private Recorder recorder;

  private double playbackStartTime, playbackLength;

  void Awake () {
    loopers = new List<Looper>();
    recorder = GetComponent<Recorder>();
    playbackStartTime = 0.0;
  }

  void OnEnable () {
    GvrReticle.OnGazePointerDown += OnGazePointerDown;
    GvrReticle.OnGazePointerUp += OnGazePointerUp;
  }

  void OnDisable () {
    GvrReticle.OnGazePointerDown -= OnGazePointerDown;
    GvrReticle.OnGazePointerUp -= OnGazePointerUp;
  }

  private void OnGazePointerDown (GameObject targetObject) {
    if (targetObject == null && !recorder.IsRecording()) {
      // Start recording.
      currentLooper = GameObject.Instantiate(looperPrefab).GetComponent<Looper>();
      currentLooper.SetTransform(Camera.main.transform, Vector3.zero);
      loopers.Add(currentLooper);
      recorder.StartRecording(currentLooper.source, OnFinishRecord);
    }
  }

  private void OnGazePointerUp (GameObject targetObject) {
    if (recorder.IsRecording()) {
      // Stop recording.
      recorder.StopRecording();
    }
  }

  private void OnFinishRecord (double startTime, double length) {
    if (loopers.Count == 1) {
      playbackStartTime = startTime;
      playbackLength = length;
      currentLooper.source.timeSamples = 
        (int)(currentLooper.source.clip.frequency * ((AudioSettings.dspTime - length) - startTime));
    }
    currentLooper.TrimAudioClip(playbackLength);

    int playbackOffset = loopers[0].source.timeSamples;
    double startOffset = startTime - playbackStartTime;
    startOffset -= playbackLength * System.Math.Floor(startOffset / playbackLength);
    currentLooper.StartPlayback(startOffset, playbackOffset);
  }
}
