using UnityEngine;
using System.Collections;

public class MetronomeVisualizer : MonoBehaviour {
  // Sequencer.
  public Sequencer sequencer;

  // Colors for metronome clicks.
  public Color blinkColor, fadeColor;

  public GameObject[] walls;

  void OnEnable () {
    sequencer.OnNextBeat += OnNextBeat;
  }

  void OnDisable () {
    sequencer.OnNextBeat -= OnNextBeat;
  }

  void Update () {
    for (int i = 0; i < walls.Length; ++i) {
      Color currentColor = walls[i].GetComponent<Renderer>().material.color;
      walls[i].GetComponent<Renderer>().material.color = Color.Lerp(currentColor, fadeColor,
                                                                    2 * Time.deltaTime);
    }
  }

  void OnNextBeat (int bar, int beat, double dspTime) {
    walls[beat % walls.Length].GetComponent<Renderer>().material.color = blinkColor;
  }
}
