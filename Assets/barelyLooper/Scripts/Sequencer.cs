using UnityEngine;
using System.Collections;

// Step sequencer that sends signals in each beat.
public class Sequencer : MonoBehaviour {
  // Sequencer event dispatcher.
  public delegate void SequencerEvent(int bar, int beat, double dspTime);
  public event SequencerEvent OnNextBeat;

  // Bar length in samples.
  [HideInInspector]
  public double barLength = 0.0;

  // Initial tempo of the sequencer, will only be used during initial setup (to be overwritten).
  [Range(72, 220)]
  public double initialTempo = 120;

  // Bars per section.
  [Range(1, 16)]
  public int numBars = 4;

  // Beats per bar.
  [Range(1, 32)]
  public int numBeats = 4;

  private IEnumerator updateCoroutine;

  // Current state of the sequencer.
  private int currentBar = -1;
  private int currentBeat = -1;

  // Beat length in samples.
  private double beatLengthSeconds {
    get { return barLength / numBeats; }
  }

  void Awake () {
    Stop();
    SetTempo(initialTempo);
  }

  public void SetTempo (double tempo) {
    barLength = 240.0 / tempo;
  }

  public void Play (double time) {
    if (updateCoroutine == null) {
      updateCoroutine = SequencerUpdateLoop(time);
      StartCoroutine(updateCoroutine);
    }
  }

  public void Stop () {  
    if (updateCoroutine != null) {
      StopCoroutine(updateCoroutine);
      updateCoroutine = null;
    }
    currentBar = -1;
    currentBeat = -1;
  }

  private IEnumerator SequencerUpdateLoop (double dspTime) {
    while (true) {
      yield return new WaitUntilDspTime(dspTime);

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

      dspTime += beatLengthSeconds;
    }
  }

  // Custom yield instruction to wait until given dsp time in seconds has been reached.
  private class WaitUntilDspTime : CustomYieldInstruction {
    // Target dsp time in seconds.
    private double dspTime;

    public override bool keepWaiting {
      get { return AudioSettings.dspTime < dspTime; }
    }

    public WaitUntilDspTime(double time) {
      dspTime = time;
    }
  }
}