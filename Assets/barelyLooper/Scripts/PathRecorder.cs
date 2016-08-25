using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathRecorder : MonoBehaviour {

  public double sampleInterval = 0.125;

  private Transform targetObject;

  private List<Vector3> path;

  private bool isRecording;

  void Awake () { 
    path = new List<Vector3>();
  }

  public void StartRecording(Transform target, double startTime) {
    targetObject = target;
    path.Clear();
    isRecording = true;
    StartCoroutine(CaptureSamplePoints(startTime));
  }

  public Vector3[] StopRecording() {
    isRecording = false;
    path.Add(targetObject.position);
    return path.ToArray();
  }

  private IEnumerator CaptureSamplePoints (double dspTime) {
    while (isRecording) {
      yield return new WaitWhile(() => AudioSettings.dspTime < dspTime);

      path.Add(targetObject.position);
      dspTime += sampleInterval;
    }
  }
}
