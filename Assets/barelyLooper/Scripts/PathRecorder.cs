using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathRecorder : MonoBehaviour {
  public Path path;

  public bool isRecording;

  public double recordStartTime;

  private static double sampleInterval = 0.2;

  private Transform targetObject;

  public void StartRecording (Transform target, double startTime) {
    targetObject = target;
    recordStartTime = startTime;
    isRecording = true;
    path = new Path();
    StartCoroutine(CaptureSamplePoints(startTime));
  }

  public void StopRecording (double stopTime) {
    isRecording = false;
    path.AddKey((float)stopTime, targetObject.position);
  }

  private IEnumerator CaptureSamplePoints (double dspTime) {
    while (isRecording) {
      yield return new WaitWhile(() => AudioSettings.dspTime < dspTime);

      path.AddKey((float)dspTime, targetObject.position);
      dspTime += sampleInterval;
    }
  }
}
