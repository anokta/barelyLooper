using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class that stores a curved path structure.
[System.Serializable]
public class Path {
  // Captured sample point.
  [System.Serializable]
  public struct Key {
    public Key(int timeSamples, Vector3 point) {
      this.timeSamples = timeSamples;
      this.point = point;
    }

    // Capture time in samples.
    public int timeSamples;

    // Key point.
    public Vector3 point;
  };

  // Number of keys in path.
  public int NumKeys {
    get { 
      if (keys != null) {
        return keys.Count;
      }
      return 0;
    }
  }

  // Sample points.
  public List<Key> keys;

  // Path length in samples.
  private int lengthSamples;

  public Path() {
    keys = new List<Key>();
    lengthSamples = -1;
  }

  // Adds key |point| at given |timeSamples|.
  public void AddKey (int timeSamples, Vector3 point) {
    keys.Add(new Key(timeSamples, point));
  }

  // Returns the key point at given |index|.
  public Vector3 GetKey (int index) {
    return keys[index].point;
  }

  // Returns the time at given |index|.
  public int GetTime (int index) {
    return keys[index].timeSamples;
  }

  public void SetLengthSamples(int pathLengthSamples) {
    lengthSamples = pathLengthSamples;
    keys.Sort((lhs, rhs) => { return lhs.timeSamples.CompareTo(rhs.timeSamples); });
  }

  // Returns the interpolated value at specified |timeSamples|.
  public Vector3 Evaluate (int timeSamples) {
    // Find the samples to interpolate between.
    int k0 = 0;
    for(int key = 0; key < NumKeys; ++key) {
      if(timeSamples < keys[key].timeSamples) {
        k0 = (key - 2 + NumKeys) % NumKeys;
        break;
      } else if(key == NumKeys - 1) {
        k0 = (key - 1 + NumKeys) % NumKeys;
      }
    }
    int k1 = (k0 + 1) % NumKeys;
    int k2 = (k0 + 2) % NumKeys;
    int k3 = (k0 + 3) % NumKeys;

    // Compute centripetal Catmull-Rom interpolation.
    Vector3 p0 = keys[k0].point;
    Vector3 p1 = keys[k1].point;
    Vector3 p2 = keys[k2].point;
    Vector3 p3 = keys[k3].point;

    float t0 = 0.0f;
    float t1 = 1.0f;
    float t2 = 2.0f;
    float t3 = 3.0f;
//    float t1 = Mathf.Pow((p1 - p0).sqrMagnitude, 0.25f) + t0;
//    float t2 = Mathf.Pow((p2 - p1).sqrMagnitude, 0.25f) + t1;
//    float t3 = Mathf.Pow((p3 - p2).sqrMagnitude, 0.25f) + t2;

    int startTimeSamples = keys[k1].timeSamples;
    int endTimeSamples = keys[k2].timeSamples;
    int interval = (endTimeSamples - startTimeSamples + lengthSamples) % lengthSamples;
    int progress = (timeSamples - startTimeSamples + lengthSamples) % lengthSamples;
    float t = interval > 0.0f ? t1 + (t2 - t1) * progress / interval : 0.0f;
    if(t > 2.0f) {
      Debug.Log("E: " + endTimeSamples  + 
                " -- S: " + startTimeSamples  +
                " -- T: " + timeSamples + 
                " -- k1: " + k1 + " -- k2: " + k2);
    }

    Vector3 a1 = ((t1 - t) * p0 + (t - t0) * p1) / (t1 - t0);
    Vector3 a2 = ((t2 - t) * p1 + (t - t1) * p2) / (t2 - t1);
    Vector3 a3 = ((t3 - t) * p2 + (t - t2) * p3) / (t3 - t2);
    Vector3 b1 = ((t2 - t) * a1 + (t - t0) * a2) / (t2 - t0);
    Vector3 b2 = ((t3 - t) * a2 + (t - t1) * a3) / (t3 - t1);
    Vector3 c = ((t2 - t) * b1 + (t - t1) * b2) / (t2 - t1);

    return c;
  }
}
