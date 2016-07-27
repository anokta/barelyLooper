using UnityEngine;
using System.Collections;

public class Metronome : MonoBehaviour {
  // Sequencer.
  public Sequencer sequencer;

  // Audio source to play metronome.
  public GvrAudioSource source;

  // Audio clips for metronome clicks.
  public AudioClip clickAccent, clickDefault;

  private AudioClip currentClip = null;
  private bool nextBeatTriggered = false;

  void OnEnable () {
    sequencer.OnNextBeat += OnNextBeat;
  }

  void OnDisable () {
    sequencer.OnNextBeat -= OnNextBeat;
  }

  // Update is called once per frame
  void Update () {
    if (nextBeatTriggered) {
      nextBeatTriggered = false;
      source.clip = currentClip;
      source.Play();
    }
  }

  void OnNextBeat (int beat) {
    nextBeatTriggered = true;
    if (beat == 0) {
      currentClip = clickAccent;
    } else {
      currentClip = clickDefault;
    }
  }
}
