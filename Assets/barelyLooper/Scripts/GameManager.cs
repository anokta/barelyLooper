﻿using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
  void Update () {
    if (Input.GetKeyDown(KeyCode.Escape) || GvrViewer.Instance.BackButtonPressed) {
      Application.Quit();
    }
  }
}
