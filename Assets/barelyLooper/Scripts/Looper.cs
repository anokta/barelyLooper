using UnityEngine;
using System.Collections;

public class Looper : MonoBehaviour {
  [HideInInspector]
  public double startTime = 0.0;

  // Audio source.
  private GvrAudioSource source;

  void Awake () {
    source = GetComponent<GvrAudioSource>();
  }

  void LateUpdate() {
    GvrViewer.Instance.UpdateState();
  }

  // Starts loop playback at given dsp time.
  public void StartPlayback (double dspTime) {
    source.PlayScheduled(dspTime);
  }
}
