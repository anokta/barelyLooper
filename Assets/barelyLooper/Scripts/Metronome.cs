using UnityEngine;
using System.Collections;

public class Metronome : MonoBehaviour {
  // Sequencer.
  public Sequencer sequencer;

  // Audio source to play metronome.
  public GvrAudioSource source;

  // Audio clips for metronome clicks.
  public AudioClip clickAccent, clickDefault;

  void OnEnable () {
    sequencer.OnNextBeat += OnNextBeat;
  }

  void OnDisable () {
    sequencer.OnNextBeat -= OnNextBeat;
  }

  void OnNextBeat (int bar, int beat, double dspTime) {
    if (beat == 0) {
      source.clip = clickAccent;
    } else {
      source.clip = clickDefault;
    }
    source.PlayScheduled(dspTime);
  }
}
