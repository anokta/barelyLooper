using UnityEngine;
using System.Collections;

public class Looper : MonoBehaviour {
  // Audio source.
  public AudioSource source;

  // Loop data in samples.
  private float[] data;

  private static int fadeSampleLength = 1024;

  void Awake () {
    source = GetComponent<AudioSource>();
  }

  public void TrimAudioClip (double length) {
    // Get the original data.
    float[] originalData = new float[source.clip.samples];
    source.clip.GetData(originalData, 0);
    // Fill in the loop data.
    int lengthSamples = (int)(length * source.clip.frequency);
    data = new float[lengthSamples];
    for(int i = 0; i < lengthSamples; ++i) {
      data[i] = originalData[i];
    }
    // Smoothen both ends.
    for (int i = 0; i < fadeSampleLength; ++i) {
      float fade = 1.0f / Mathf.Sqrt(i + 1);
      data[i] *= fade;
      data[lengthSamples - i - 1] *= fade;
    }
    // Set loop data as the new clip.
    source.clip = AudioClip.Create("Loop", lengthSamples, 1, source.clip.frequency, false);
    source.clip.SetData(data, 0);
  }

  // Starts loop playback at given dsp time.
  public void StartPlayback (double startOffset, int playbackOffset) {
    source.timeSamples = playbackOffset;
    source.PlayScheduled(AudioSettings.dspTime + startOffset);
  }

  public float[] GetAudioData () {
    return data;
  }
}
