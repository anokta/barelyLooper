using UnityEngine;
using System.Collections;

// Microphone recorder.
public class Recorder : MonoBehaviour {
  // Recorder event dispatcher.
  public delegate void RecorderEvent(double startTime, double length, int frequency, float[] data);

  // Callback on finish recording.
  public RecorderEvent OnFinishRecord = null;

  // Maximum recording length in seconds.
  public int maxRecordLength = 20;

  // Is an audio clip is being currently recorded?
  public bool IsRecording {
    get { return recordStartTime > 0.0; }
  }

  // The record output latency in samples.
  public double RecordLatency {
    get { return outputLatency; }
  }

  // Record sampling rate.
  private int recordFrequency;

  // Audio clip to record into.
  private AudioClip recordClip;

  // Input/output latency in seconds.
  private double outputLatency;

  // Start dsp time of the current recording.
  private double recordStartTime;

  void Awake () {
    recordStartTime = 0.0;
    // Get maximum recording sampling rate of the mic.
    int minFrequency = 0, maxFrequency = 0;
    Microphone.GetDeviceCaps(null, out minFrequency, out maxFrequency);
    recordFrequency = maxFrequency;
    // Get output latency in samples.
    int bufferLength = 0, numBuffers = 0;
    AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
    outputLatency = (double)numBuffers * bufferLength / AudioSettings.outputSampleRate;
    // Initialize the audio clip to record into.
    recordClip = 
      AudioClip.Create("Record", maxRecordLength * recordFrequency, 1, recordFrequency, false);
  }

  // Starts recording a clip.
  public void StartRecording () {
    recordStartTime = AudioSettings.dspTime;
    recordClip = Microphone.Start(null, true, maxRecordLength, recordFrequency);
  }

  // Stops recording and triggers the on finish callback.
  public void StopRecording () {
    double recordEndTime = AudioSettings.dspTime;
    Microphone.End(null);
    if (OnFinishRecord != null) {
      // Trigger on finish record callback.
      double recordLength = recordEndTime - recordStartTime;
      float[] recordData = new float[(int)(recordLength * recordFrequency)];
      recordClip.GetData(recordData, 0);
      OnFinishRecord(recordStartTime, recordLength, recordFrequency, recordData);
    }
    recordStartTime = 0.0;
  }
}
