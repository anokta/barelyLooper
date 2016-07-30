using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO(anokta): Currently for debug purposes only, will be refactored later.
public class GameManager : MonoBehaviour {
  // Mic recorder.
  public Recorder recorder;

  // Looper sequencer.
  public Sequencer sequencer;

  // Prefab object to instantiate loopers.
  public GameObject looperPrefab;

  // Looper instances.
  private List<Looper> loopers;

  void Awake () {
    loopers = new List<Looper>();
  }

  void OnEnable () {
    sequencer.OnNextBeat += OnNextBeat;
  }

  void OnDisable () {
    sequencer.OnNextBeat -= OnNextBeat;
  }

  void Update () {
    if (Input.GetKeyDown(KeyCode.Space)) {
      sequencer.Play();
    }
    if (!recorder.IsRecording()) {
      if (Input.GetMouseButtonDown(0)) {
        // Start recording.
        GameObject looperObject = GameObject.Instantiate(looperPrefab);
        looperObject.GetComponent<Renderer>().material.color = Color.yellow;
        looperObject.transform.position = Camera.main.transform.rotation * (4.0f * Vector3.forward);
        loopers.Add(looperObject.GetComponent<Looper>());
        recorder.StartRecording(looperObject.GetComponent<GvrAudioSource>(), OnFinishRecord);
      } 
    } else if (Input.GetMouseButtonUp(0)) { 
      // Stop recording.
      recorder.StopRecording();
    }
  }

  private void OnFinishRecord (double length) {
    if (loopers.Count == 1) {
      sequencer.barLength = length;
      sequencer.Play();
    }
  }

  private void OnNextBeat (int bar, int beat, double time) {
    if (beat == 0) {
      for (int i = 0; i < loopers.Count; ++i) {
        loopers[i].StartPlayback(time);
      }
    }
  }
}
