using UnityEngine;
using System.Collections;

// Class that stores a curved path structure.
public class LooperPath {
  // Number of keyframes in path.
  public int Length {
    get { 
      if (positionCurves != null && positionCurves.Length > 0) {
        return positionCurves[0].length;
      }
      return 0;
    }
  }

  // Animation curve per each transform axis.
  private AnimationCurve[] positionCurves;

  public LooperPath() {
    // Three curves needed for X, Y & Z coordinates.
    positionCurves = new AnimationCurve[3];
    for (int axis = 0; axis < positionCurves.Length; ++axis) {
      positionCurves[axis] = new AnimationCurve();
    }
  }

  // Adds keyframe |position| at given |time|.
  public void AddKey(float time, Vector3 position) {
    positionCurves[0].AddKey(time, position.x);
    positionCurves[1].AddKey(time, position.y);
    positionCurves[2].AddKey(time, position.z);
  }

  // Returns the keyframe at given |index|.
  public Vector3 GetKey(int index) {
    return new Vector3(positionCurves[0].keys[index].value, positionCurves[1].keys[index].value,
                       positionCurves[2].keys[index].value);
  }

  // Returns the time at given |index|.
  public float GetTime(int index) {
    return positionCurves[0].keys[index].time;
  }

  // Removes keyframe at given |index|.
  public void RemoveKey(int index) {
    positionCurves[0].RemoveKey(index);
    positionCurves[1].RemoveKey(index);
    positionCurves[2].RemoveKey(index);
  }

  // Returns the interpolated position at specified |time|.
  public Vector3 Evaluate(float time) {
    return new Vector3(positionCurves[0].Evaluate(time), positionCurves[1].Evaluate(time), 
                       positionCurves[2].Evaluate(time));
  }
}
