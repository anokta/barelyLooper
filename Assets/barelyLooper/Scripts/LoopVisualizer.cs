using UnityEngine;
using System.Collections;

public class LoopVisualizer : MonoBehaviour {

  public Color quietColor = Color.green;
  public Color loudColor = Color.yellow;

  public Looper looper;

  private Color currentColor, targetColor;

  void Awake() {
    currentColor = quietColor;
    targetColor = currentColor;
  }

  // Update is called once per frame
  void Update () {
    float[] data = looper.GetAudioData();
    if (data != null) {
      int length = (int)(Time.deltaTime * AudioSettings.outputSampleRate);
      int offset = looper.source.timeSamples;

      float energy = 0.0f;
      for (int i = 0; i < length; ++i) {
        energy += Mathf.Pow(data[(offset + i) % data.Length], 2.0f);
      }

      targetColor = quietColor * (1.0f - energy) + loudColor * energy;
      currentColor = Color.Lerp(currentColor, targetColor , 10.0f * Time.deltaTime);
      looper.gameObject.GetComponent<Renderer>().material.color = currentColor;
    }
  }
}
