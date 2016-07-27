// ----------------------------------------------------------------------
//   barelyLooper - Audio Looper Toy For VR
//
//     Copyright 2016 Alper Gungormusler. All rights reserved.
//
// ------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

// Step sequencer that sends signals in each bar and beat respectively.
// Adopted from BarelyAPI.Sequencer of barelyMusician (https://github.com/anokta/barelyMusician).
public class Sequencer : MonoBehaviour {
  // Sequencer event dispatcher.
  public delegate void SequencerEvent(int state);

  // Sequencer callbacks per each audio event.
  public event SequencerEvent OnNextBar, OnNextBeat;

  // Common note length types.
  public enum NoteType {
    WholeNote = 1,
    HalfNote = 2,
    QuarterNote = 4,
    EightNote = 8,
    SixteenthNote = 16
  }

  // Beats per minute.
  public int tempo = 120;

  // Bars per section.
  public int numBars = 4;

  // Beats per bar.
  public int numBeats = 4;

  // Clock frequency per bar.
  public int numPulsesInBar = 64;

  // Note length type.
  public NoteType noteType = NoteType.QuarterNote;

  // System sampling rate.
  private int sampleRate = 44100;

  // Source that provides the audio callback.
  private AudioSource audioSource = null;

  // Current state of the sequencer.
  private int currentBar = -1;
  private int currentBeat = -1;
  private int currentPulse = -1;

  // Granular counter to determine the current state.
  private float phasor = 0;

  // Bar length in pulses.
  private int BarLength {
    get { return numBeats * BeatLength; }
  }

  // Beat length in pulses.
  private int BeatLength {
    get { return numPulsesInBar / (int)noteType; }
  }

  // Pulse interval.
  private float PulseInterval {
    get { return 240.0f * sampleRate / numPulsesInBar / tempo; }
  }

  void Awake () {
    sampleRate = AudioSettings.outputSampleRate;
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.playOnAwake = false;
    audioSource.hideFlags = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
    audioSource.bypassListenerEffects = true;
    audioSource.bypassReverbZones = true;
    audioSource.spatialBlend = 0.0f;
    Stop();
  }

  public void Play () { 
    audioSource.Play();
  }

  public void Pause () {
    audioSource.Pause();
  }

  public void Stop () { 
    audioSource.Stop();

    currentBar = -1;
    currentBeat = -1;
    currentPulse = -1;

    phasor = PulseInterval;
  }

  void OnAudioFilterRead (float[] data, int channels) {
    // Update |phasor| by number of frames in |data|.
    phasor += data.Length / channels;

    // Trigger next audio events.
    int numPulses = Mathf.FloorToInt(phasor / PulseInterval);
    for (int pulse = 0; pulse < numPulses; ++pulse) {
      currentPulse = (currentPulse + 1) % BarLength;
      if (currentPulse % BeatLength == 0) {
        // Next beat.
        currentBeat = (currentBeat + 1) % numBeats;
        if (currentBeat == 0) {
          // Next bar.
          currentBar = (currentBar + 1) % numBars;
          TriggerNextBar(currentBar);
        }
        TriggerNextBeat(currentBeat);
      }
    }
    phasor -= numPulses * PulseInterval;
  }

  // Bar callback function.
  void TriggerNextBar (int bar) {
    if (OnNextBar != null) {
      OnNextBar(bar);
    }
  }

  // Beat callback function.
  void TriggerNextBeat (int beat) {
    if (OnNextBeat != null) {
      OnNextBeat(beat);
    }
  }
}