using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
  public Sequencer sequencer;

  void Update () {
    if (Input.GetKeyDown(KeyCode.Escape) || GvrViewer.Instance.BackButtonPressed) {
      Application.Quit();
    }

    // Debug only.
    if(Input.GetKeyDown(KeyCode.Space)) {
      sequencer.Play(AudioSettings.dspTime);
    }
    if(Input.GetKeyDown(KeyCode.Return)) {
      sequencer.Stop();
    }
  }
}
