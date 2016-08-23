using UnityEngine;
using System.Collections;

// Basic metronome that ticks in each sequenced beat of a bar.
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

  // Implements |Sequencer.OnNextBeat| callback.
  void OnNextBeat (int bar, int beat, double dspTime) {
    if (beat == 0) {
      source.clip = clickAccent;
    } else {
      source.clip = clickDefault;
    }
    source.PlayScheduled(dspTime);
  }
}
