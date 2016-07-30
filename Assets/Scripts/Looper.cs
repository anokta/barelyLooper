using UnityEngine;
using System.Collections;

public class Looper : MonoBehaviour {
  // Audio source.
  private GvrAudioSource source;

  void Awake () {
    source = GetComponent<GvrAudioSource>();
  }
	
  // Starts loop playback at given dsp time.
  public void StartPlayback (double dspTime) {
    source.PlayScheduled(dspTime);
  }
}
