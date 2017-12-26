using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathRecorder : MonoBehaviour {
  // Record sample update interval.
  public double updateInterval = 0.2;

  // Next record update time in seconds.
  private double nextUpdateTime;

  // Record path.
  private LooperPath recordPath;

  // Record start time in seconds.
  private double recordStartTime;

  // Target object to record.
  private Transform recordTarget;

  // Record sampling frequency.
  private int samplingFrequency;

  void Awake() {
    nextUpdateTime = 0.0;
    recordStartTime = 0.0;
    recordTarget = null;
    samplingFrequency = 0;
  }

  void Update() {
    if (recordTarget != null) {
      // Check if the next sample point needs to be recorded.
      double dspTime = AudioSettings.dspTime;
      if (dspTime >= nextUpdateTime) {
        // Add the next sample point to the path.
        recordPath.AddKey((float) ((dspTime - recordStartTime) * samplingFrequency), 
                          recordTarget.position);
        nextUpdateTime += updateInterval;
      }
    }
  }

  public void StartRecording(double startTime, int frequency, Transform target) {
    recordPath = new LooperPath();
    recordStartTime = startTime;
    nextUpdateTime = recordStartTime;
    samplingFrequency = frequency;
    recordTarget = target;
  }

  public LooperPath StopRecording(double endTime) {
    // Add the last sample point to the path.
    recordPath.AddKey((float) ((endTime - recordStartTime) * samplingFrequency), 
                      recordTarget.position);
    recordStartTime = 0.0;
    recordTarget = null;
    return recordPath;
  }
}
