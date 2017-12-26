using UnityEngine;
using System.Collections;

// Step sequencer that sends signals in each beat.
public class Sequencer : MonoBehaviour {
  // Sequencer event dispatcher.
  public delegate void SequencerEvent(int bar, int beat, double dspTime);

  public event SequencerEvent OnNextBeat;

  // Bars per section.
  [Range(1, 16)]
  public int numBars = 4;

  // Beats per bar.
  [Range(1, 32)]
  public int numBeats = 4;

  public bool IsPlaying {
    get { return (updateCoroutine != null); }
  }

  // Coroutine to update the sequencer state.
  private IEnumerator updateCoroutine;

  // Current state of the sequencer.
  private int currentBar;
  private int currentBeat;

  // Bar length in seconds.
  private double barLength;

  // Beat length in samples.
  private double beatLength {
    get { return barLength / numBeats; }
  }

  void Awake() {
    currentBar = -1;
    currentBeat = -1;
    barLength = 0.0;
  }

  // Starts the sequencer playback.
  public void Play(double startTime, double barLengthSeconds) {
    if (!IsPlaying) {
      barLength = barLengthSeconds;
      updateCoroutine = UpdateLoop(startTime);
      StartCoroutine(updateCoroutine);
    }
  }

  // Stops the sequencer playback.
  public void Stop() {  
    if (IsPlaying) {
      StopCoroutine(updateCoroutine);
      updateCoroutine = null;
      currentBar = -1;
      currentBeat = -1;
    }
  }

  // Sets the tempo (bpm).
  public void SetTempo(double tempo) {
    barLength = 240.0 / tempo;
  }

  // Sequencer update coroutine.
  private IEnumerator UpdateLoop(double dspTime) {
    while (IsPlaying) {
      // Wait till next beat.
      yield return new WaitWhile(() => AudioSettings.dspTime < dspTime);

      // Next beat.
      currentBeat = (currentBeat + 1) % numBeats;
      if (currentBeat == 0) {
        // Next bar.
        currentBar = (currentBar + 1) % numBars;
      }
      // Trigger next beat callback.    
      if (OnNextBeat != null) {
        OnNextBeat(currentBar, currentBeat, dspTime);
      }

      // Update next beat time.
      dspTime += beatLength;
    }
  }
}