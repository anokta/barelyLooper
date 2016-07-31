// ----------------------------------------------------------------------
//   barelyLooper - Audio Looper Toy For VR
//
//     Copyright 2016 Alper Gungormusler. All rights reserved.
//
// ------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

// Step sequencer that sends signals in each beat.
// Adopted from BarelyAPI.Sequencer of barelyMusician (https://github.com/anokta/barelyMusician).
public class Sequencer : MonoBehaviour {
  // Sequencer event dispatcher.
  public delegate void SequencerEvent(int bar, int beat, double dspTime);
  public event SequencerEvent OnNextBeat;

  // Bar length in samples.
  [HideInInspector]
  public double barLength = 0.0;

  // Initial tempo of the sequencer, will only be used during initial setup (to be overwritten).
  [Range(72, 220)]
  public double initialTempo = 120.0;

  // Bars per section.
  [Range(1, 16)]
  public int numBars = 4;

  // Beats per bar.
  [Range(1, 32)]
  public int numBeats = 4;

  // Current state of the sequencer.
  private int currentBar = -1;
  private int currentBeat = -1;

  // Is sequencer playing?
  private bool isPlaying = false;

  // Time in samples to determine when the next beat should be triggered.
  private double nextBeatTime = 0.0;

  // Beat length in samples.
  private double beatLength {
    get { return barLength / numBeats; }
  }

  void Awake () {
    Stop();
    SetTempo(initialTempo);
  }

  void Update () {
    if (!isPlaying) {
      // Skip processing if not playing.
      return;
    }

    // Get the current dsp time in samples.
    double currentTime = AudioSettings.dspTime;
    if (currentTime >= nextBeatTime) {
      // Trigger next beat.
      currentBeat = (currentBeat + 1) % numBeats;
      if (currentBeat == 0) {
        // Trigger next bar.
        currentBar = (currentBar + 1) % numBars;
        TriggerNextBar(nextBeatTime);
      }
      TriggerNextBeat(nextBeatTime);
      // Update the next beat time.
      nextBeatTime += beatLength;
    }
  }

  public void SetTempo (double tempo) {
    barLength = 240.0 / tempo;
  }

  public void Play (double time) {
    nextBeatTime = time;
    isPlaying = true;
  }

  public void Stop () {  
    isPlaying = false;
    currentBar = -1;
    currentBeat = -1;
    nextBeatTime = 0.0;
  }

  // Bar callback function.
  private void TriggerNextBar (double dspTime) {
    if (OnNextBeat != null) {
      OnNextBeat(currentBar, currentBeat, dspTime);
    }
  }

  // Beat callback function.
  private void TriggerNextBeat (double dspTime) {
    if (OnNextBeat != null) {
      OnNextBeat(currentBar, currentBeat, dspTime);
    }
  }
}