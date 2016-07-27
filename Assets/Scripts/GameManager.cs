using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
  // Looper sequencer.
  public Sequencer sequencer;

  void Update () {
    if (Input.GetKeyDown(KeyCode.Space)) {
      sequencer.Play();
    }
  }
}
