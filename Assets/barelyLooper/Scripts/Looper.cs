using UnityEngine;
using System.Collections;

public class Looper : MonoBehaviour {
  // Audio source.
  public GvrAudioSource source;

  // Loop data in samples.
  private float[] data;

  private static int fadeSampleLength = 1024;

  void Awake () {
    source = GetComponent<GvrAudioSource>();
  }

  public void TrimAudioClip (double length) {
    int lengthSamples = (int)(length * source.clip.frequency);
    // Get original data.
    data = new float[lengthSamples];
    source.clip.GetData(data, 0);
    // Smoothen both ends.
    for(int i = 0; i < fadeSampleLength; ++i) {
      float fade = 1.0f / Mathf.Sqrt(i + 1);
      data[i] *= fade;
      data[lengthSamples - i - 1] *= fade;
    }
    // Set trimmed data as the new clip.
    source.clip = AudioClip.Create("Loop", lengthSamples, 1, source.clip.frequency, false);
    source.clip.SetData(data, 0);
  }

  // Starts loop playback at given dsp time.
  public void StartPlayback (double dspTime) {
    source.PlayScheduled(dspTime);
  }

  public float[] GetAudioData() {
    return data;
  }
}
