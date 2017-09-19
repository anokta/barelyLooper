using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
  void Awake () {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
  }

  void Update () {
    if (Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
  }
}
