using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathRecorder : MonoBehaviour {

  public double sampleInterval = 0.125;

  private Transform targetObject;

  private Path path;

  private bool isRecording;

  public void StartRecording(Transform target, double startTime) {
    targetObject = target;
    path = new Path();
    isRecording = true;
    StartCoroutine(CaptureSamplePoints(startTime));
  }

  public Path StopRecording(double stopTime) {
    isRecording = false;
    path.AddKey((float)stopTime, targetObject.position);
    return path;
  }

  private IEnumerator CaptureSamplePoints (double dspTime) {
    while (isRecording) {
      yield return new WaitWhile(() => AudioSettings.dspTime < dspTime);

      path.AddKey((float)dspTime, targetObject.position);
      dspTime += sampleInterval;
    }
  }
}
