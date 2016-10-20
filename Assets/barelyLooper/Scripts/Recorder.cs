using UnityEngine;
using System.Collections;

// Microphone recorder.
public class Recorder {
  // Record sampling rate.
  public int frequency;

  // Input/output latency in seconds.
  public double outputLatency;

  // Is an audio clip is being currently recorded?
  public bool IsRecording {
    get { return Microphone.IsRecording(null); }
  }

  // Audio clip to record into.
  private AudioClip recordClip;

  // Record data.
  private float[] recordData;

  // Maximum recording length in seconds.
  private int recordLength;

  // Constructs a new recorder with |maxRecordLength|.
  public Recorder(int maxRecordLength) {
    // Get maximum recording sampling rate of the mic.
    int minFrequency = 0, maxFrequency = 0;
    Microphone.GetDeviceCaps(null, out minFrequency, out maxFrequency);
    frequency = maxFrequency;
    // Get output latency in samples.
    int bufferLength = 0, numBuffers = 0;
    AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
    outputLatency = (double)numBuffers * bufferLength / AudioSettings.outputSampleRate;
    // Initialize the audio clip to record into.
    recordLength = maxRecordLength;
    int recordLengthSamples = recordLength * frequency;
    recordClip = AudioClip.Create("Record", recordLengthSamples, 1, frequency, false);
    recordData = new float[recordLengthSamples];
  }

  // Starts recording a clip.
  public void StartRecording () {
    recordClip = Microphone.Start(null, true, recordLength, frequency);
  }

  // Stops recording and returns the recorded audio data.
  public float[] StopRecording () {
    Microphone.End(null);
    recordClip.GetData(recordData, 0);
    return recordData;
  }
}
