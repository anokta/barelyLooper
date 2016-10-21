using UnityEngine;
using System.Collections;

// Microphone recorder.
public class Recorder : MonoBehaviour {
  // Maximum loop length in seconds.
  public int maxRecordLength = 30;

  // Record sampling rate.
  public int Frequency {
    get { return recordFrequency; }
  }
  private int recordFrequency;
 
  // Audio clip to record into.
  private AudioClip recordClip;

  // Record data.
  private float[] recordData;

  // Maximum recording length in samples.
  private int recordLengthSamples;

  // Initial start time in seconds.
  private double initialStartTime;
 
  void OnEnable() {
    // Get maximum recording sampling rate of the mic.
    int minFrequency = 0, maxFrequency = 0;
    Microphone.GetDeviceCaps(null, out minFrequency, out maxFrequency);
    recordFrequency = maxFrequency;
    // Start recording in the background.
    recordLengthSamples = maxRecordLength * recordFrequency;
    recordData = new float[recordLengthSamples];
    initialStartTime = AudioSettings.dspTime;
    recordClip = Microphone.Start(null, true, maxRecordLength, recordFrequency);
  }

  void OnDisable() {
    // Stop recording on shutdown.
    Microphone.End(null);
  }

  // Stops recording and returns the recorded audio data from |startTime| to |endTime|.
  public float[] GetRecordedData(double startTime, double endTime) {
    int lengthSamples = (int)((endTime - startTime) * recordFrequency);
    float[] data = new float[lengthSamples];
    // Calculate the recording latency.
    int elapsedSamples = (int)(recordFrequency * (endTime - initialStartTime));
    int currentPosition = Microphone.GetPosition(null);
    int latencySamples = (elapsedSamples - currentPosition) % recordLengthSamples;
    // Compute the start position relative to the cursor.
    int startPosition = currentPosition - (lengthSamples - latencySamples);
    startPosition = (startPosition + recordLengthSamples) % recordLengthSamples;
    // Fill in the recorded data. 
    // Manually traverse through the whole data to workaround the |AudioClip.GetData| wrapping bug.
    recordClip.GetData(recordData, 0);
    for(int i = 0; i < data.Length; ++i) {
      if(i < data.Length - latencySamples) {
        data[i] = recordData[(startPosition + i) % recordLengthSamples];
      } else {
        // Fill the rest with zeros to compensate the non-existent samples to be recorded.
        // TODO(#28): Start a coroutine here to wait for the |latencySamples| instead.
        data[i] = 0.0f;
      }
    }
    return data;
  }
}
