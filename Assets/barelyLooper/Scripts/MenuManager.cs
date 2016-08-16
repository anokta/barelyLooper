using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour {

  public Toggle metronomeToggle;
  public Slider tempoSlider;
  public Slider beatsSlider;

  public Metronome metronome;

  public Sequencer sequencer;


  public void SetTempo() {
    sequencer.SetTempo(tempoSlider.value);
  }

  public void SetBeats() {
    sequencer.numBeats = (int)beatsSlider.value;
  }

  public void ToggleMetronome() {
    metronome.enabled = metronomeToggle.isOn;
  }
}
