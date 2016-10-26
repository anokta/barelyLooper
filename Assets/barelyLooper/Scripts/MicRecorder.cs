using UnityEngine;
using System.Collections;

// Microphone recorder.
public class MicRecorder : MonoBehaviour {
  // Maximum loop length in seconds.
  public int maxRecordLength = 30;

  // Record sampling rate.
  public int Frequency {
    get { return recordFrequency; }
  }
 
  // Audio clip to record into.
  private AudioClip recordClip;

  // Record data.
  private float[] recordData;

  // Record sample frequency in Hz.
  private int recordFrequency;

  // Maximum recording length in samples.
  private int recordLengthSamples;

  // Initial start time in seconds.
  private double initialStartTime;

  void Awake () {
    // Get maximum recording sampling rate of the mic.
    int minFrequency = 0, maxFrequency = 0;
    Microphone.GetDeviceCaps(null, out minFrequency, out maxFrequency);
    recordFrequency = maxFrequency;
    // Set maximum recording length.
    recordLengthSamples = maxRecordLength * recordFrequency;
    recordData = new float[recordLengthSamples];
  }

  void OnEnable () {
    // Start recording in the background.
    StartRecording();
  }

  void OnDisable () {
    // Stop recording on shutdown.
    StopRecording();
  }

  void OnApplicationPause (bool pauseStatus) {
    if (pauseStatus) {
      StopRecording();
    } else {
      StartRecording();
    }
  }

  // Fills in the recorded audio |data| from |startTime| to |endTime| and returns the current
  // latency in samples. An additional latency samples will be added to the final record data.
  //
  // @param startTime Record start time in seconds.
  // @param endtime Record end time in seconds.
  // @param data Record data to be filled in.
  // @return Current record latency in samples.
  public int GetRecordedData (double startTime, double endTime, out float[] data) {
    // Calculate the recording latency.
    int elapsedSamples = (int)((endTime - initialStartTime) * recordFrequency);
    int currentPosition = Microphone.GetPosition(null);
    int latencySamples = (elapsedSamples - currentPosition) % recordLengthSamples;
    // Initialize the record data.
    int lengthSamples = (int)((endTime - startTime) * recordFrequency) + latencySamples;
    lengthSamples = Mathf.Min(lengthSamples, recordLengthSamples); 
    data = new float[lengthSamples];
    // Compute the start position relative to the cursor offset with |latencySamples|.
    int startPosition = currentPosition - lengthSamples;
    startPosition = (startPosition + recordLengthSamples) % recordLengthSamples;
    // Fill in the recorded data. 
    // Manually traverse through the whole data to workaround the |AudioClip.GetData| wrapping bug.
    recordClip.GetData(recordData, 0);
    for (int i = 0; i < data.Length; ++i) {
      data[i] = recordData[(startPosition + i) % recordLengthSamples];
    }
    return latencySamples;
  }

  // Starts the mic recording with |maxRecordLength|.
  private void StartRecording () {
    if (!Microphone.IsRecording(null)) {
      initialStartTime = AudioSettings.dspTime;
      recordClip = Microphone.Start(null, true, maxRecordLength, recordFrequency);
    }
  }

  // Stops the mic recording.
  private void StopRecording () {
    if (Microphone.IsRecording(null)) {
      Microphone.End(null);
    }
  }
}
