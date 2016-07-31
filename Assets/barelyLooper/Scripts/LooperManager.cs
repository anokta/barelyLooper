using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LooperManager : MonoBehaviour {
  // Looper sequencer.
  public Sequencer sequencer;

  // Prefab object to instantiate loopers.
  public GameObject looperPrefab;

  // Looper instances.
  private List<Looper> loopers;

  // Mic recorder.
  private Recorder recorder;

  void Awake () {
    loopers = new List<Looper>();
    recorder = GetComponent<Recorder>();
  }

  void OnEnable () {
    GvrReticle.OnGazePointerDown += OnTouchDown;
    GvrReticle.OnGazePointerUp += OnTouchUp;
    sequencer.OnNextBeat += OnNextBeat;
  }

  void OnDisable () {
    GvrReticle.OnGazePointerDown -= OnTouchDown;
    GvrReticle.OnGazePointerUp -= OnTouchUp;
    sequencer.OnNextBeat -= OnNextBeat;
  }

  void Update () {
    if (Input.GetKeyDown(KeyCode.Space)) {
      sequencer.Play(AudioSettings.dspTime);
    }
  }

  private void OnTouchDown (GameObject targetObject) {
    Debug.Log("OnTouchDown");
    if (targetObject != null) {
      //Looper selectedLooper = EventSystem.current.currentSelectedGameObject.GetComponent<Looper>();
      // Drag & drop .....
    } else if (!recorder.IsRecording()) {
      // Start recording.
      GameObject looperObject = GameObject.Instantiate(looperPrefab);
      looperObject.transform.position = Camera.main.transform.rotation * (2.0f * Vector3.forward);
      loopers.Add(looperObject.GetComponent<Looper>());
      recorder.StartRecording(looperObject.GetComponent<GvrAudioSource>(), OnFinishRecord);
    }
  }

  private void OnTouchUp (GameObject targetObject) {
    Debug.Log("OnTouchUp");
    if (recorder.IsRecording()) {
      recorder.StopRecording();
    } else if (targetObject != null) {
      loopers.Remove(targetObject.GetComponent<Looper>());
      GameObject.Destroy(targetObject);
    } 
  }

  private void OnFinishRecord (double startTime, double length) {
    if (loopers.Count == 1) {
      sequencer.barLength = length;
      sequencer.Play(startTime);
    }
    loopers[loopers.Count - 1].startTime = startTime;
  }

  private void OnNextBeat (int bar, int beat, double time) {
    if (beat == 0) {
      for (int i = 0; i < loopers.Count; ++i) {
        loopers[i].StartPlayback(time);
      }
    }
  }
}
