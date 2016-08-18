using UnityEngine;
using System.Collections;

public class MetronomeVisualizer : MonoBehaviour {
  // Sequencer.
  public Sequencer sequencer;

  // Colors for metronome clicks.
  public Color blinkColor, fadeColor;

  public GameObject parentObject;
  private Renderer[] children;

  void Awake() {
    children = parentObject.GetComponentsInChildren<Renderer>();
  }

  void OnEnable () {
    sequencer.OnNextBeat += OnNextBeat;
  }

  void OnDisable () {
    sequencer.OnNextBeat -= OnNextBeat;
  }

  void Update () {
    for (int i = 0; i < children.Length; ++i) {
      Color currentColor = children[i].material.color;
      children[i].material.color = Color.Lerp(currentColor, fadeColor, 2 * Time.deltaTime);
    }
  }

  void OnNextBeat (int bar, int beat, double dspTime) {
    for (int i = 0; i < children.Length; ++i) {
      children[i].material.color = blinkColor;
    }
  }
}