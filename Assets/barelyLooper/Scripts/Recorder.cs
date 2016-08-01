using UnityEngine;
using System.Collections;

public class Recorder : MonoBehaviour {
  // Maximum recording length in seconds.
  public int maxRecordLength = 15;

  // Recorder event dispatcher.
  public delegate void RecorderEvent(double startTime, double length);

  // Callback on finish recording.
  private RecorderEvent onFinishRecord;

  // Start dsp time of the current recording.
  private double recordStartTime;

  void Awake () {
    recordStartTime = 0.0;
  }

  void Update () {
    if (IsRecording() && !Microphone.IsRecording(null)) {
      StopRecording();
    }
  }

  // Returns whether an audio clip is being currently recorded.
  public bool IsRecording () {
    return recordStartTime > 0.0;
  }

  // Starts recording a clip.
  public void StartRecording (AudioSource source, RecorderEvent onFinishRecordCallback) {
    if (!IsRecording()) {
      recordStartTime = AudioSettings.dspTime;
      onFinishRecord = onFinishRecordCallback;
      source.clip = Microphone.Start(null, false, maxRecordLength, AudioSettings.outputSampleRate);
      while(!(Microphone.GetPosition(null) > 0));
    } else {
      Debug.LogWarning("[StartRecording] An audio clip is already being recorded.");
    }
  }

  // Stops recording and triggers the on finish callback.
  public void StopRecording () {
    if (IsRecording()) {
      double recordEndTime = AudioSettings.dspTime;
      Microphone.End(null);
      if (onFinishRecord != null) {
        onFinishRecord(recordStartTime, recordEndTime - recordStartTime);
      }
      recordStartTime = 0.0;
    } else {
      Debug.LogWarning("[StopRecording] No active recording.");
    }
  }
}
