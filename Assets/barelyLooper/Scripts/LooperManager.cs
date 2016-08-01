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

  // Mic recorder.
  private Recorder recorder;

  private double playbackStartTime, playbackLength;

  void Awake () {
    loopers = new List<Looper>();
    recorder = GetComponent<Recorder>();
    playbackStartTime = 0.0;
  }

  void OnEnable () {
    GvrReticle.OnGazePointerDown += OnTouchDown;
    GvrReticle.OnGazePointerUp += OnTouchUp;
    //sequencer.OnNextBeat += OnNextBeat;
  }

  void OnDisable () {
    GvrReticle.OnGazePointerDown -= OnTouchDown;
    GvrReticle.OnGazePointerUp -= OnTouchUp;
    //sequencer.OnNextBeat -= OnNextBeat;
  }

  private void OnTouchDown (GameObject targetObject) {
    if (targetObject != null) {
      //Looper selectedLooper = EventSystem.current.currentSelectedGameObject.GetComponent<Looper>();
      // Drag & drop .....
    } else if (!recorder.IsRecording()) {
      // Start recording.
      GameObject looperObject = GameObject.Instantiate(looperPrefab);
      looperObject.transform.position = 
        Camera.main.transform.position + Camera.main.transform.rotation * (2.0f * Vector3.forward);
      looperObject.GetComponent<Renderer>().material.color = recordColor;
      loopers.Add(looperObject.GetComponent<Looper>());
      recorder.StartRecording(looperObject.GetComponent<GvrAudioSource>(), OnFinishRecord);
    }
  }

  private void OnTouchUp (GameObject targetObject) {
    if (recorder.IsRecording()) {
      recorder.StopRecording();
    } else if (targetObject != null) {
      loopers.Remove(targetObject.GetComponent<Looper>());
      GameObject.Destroy(targetObject);
    } 
  }

  private void OnFinishRecord (double startTime, double length) {
    if (loopers.Count == 1) {
      playbackStartTime = startTime;
      playbackLength = length;
      loopers[0].source.timeSamples = 
        (int)(loopers[0].source.clip.frequency * ((AudioSettings.dspTime - length) - startTime));
    }
    loopers[loopers.Count - 1].TrimAudioClip(playbackLength);

    int playbackOffset = loopers[0].source.timeSamples;
    double startOffset = startTime - playbackStartTime;
    startOffset -= playbackLength * System.Math.Floor(startOffset / playbackLength);
    loopers[loopers.Count - 1].StartPlayback(startOffset, playbackOffset);
  }
}
