using UnityEngine;
using System.Collections;

// Class that stores a curved path structure.
public class Path {
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

  public Path() {
    // Three curves needed for X, Y & Z coordinates.
    positionCurves = new AnimationCurve[3];
    for (int i = 0; i < positionCurves.Length; ++i) {
      positionCurves[i] = new AnimationCurve();
      // Loop the path to extend its time frame.
      positionCurves[i].preWrapMode = WrapMode.Loop;
      positionCurves[i].postWrapMode = WrapMode.Loop;
    }
  }

  // Adds keyframe |position| at given |time|.
  public void AddKey (float time, Vector3 position) {
    positionCurves[0].AddKey(time, position.x);
    positionCurves[1].AddKey(time, position.y);
    positionCurves[2].AddKey(time, position.z);
  }

  // Returns the keyframe at given |index|.
  public Vector3 GetKey (int index) {
    return new Vector3(positionCurves[0].keys[index].value, positionCurves[1].keys[index].value,
                       positionCurves[2].keys[index].value);
  }

  // Returns the time at given |index|.
  public float GetTime (int index) {
    return positionCurves[0].keys[index].time;
  }

  // Returns the interpolated position at specified |time|.
  public Vector3 Evaluate (float time) {
    return new Vector3(positionCurves[0].Evaluate(time), positionCurves[1].Evaluate(time), 
                       positionCurves[2].Evaluate(time));
  }
}
