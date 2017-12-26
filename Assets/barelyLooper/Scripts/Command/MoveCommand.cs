using UnityEngine;
using System.Collections;

public class MoveCommand : Command {
  private LoopController looper;
  private Vector3 previousPosition;
  private Quaternion previousRotation;
  private Vector3 currentPosition;
  private Quaternion currentRotation;

  public MoveCommand(LoopController loop, Vector3 previousPos, Quaternion previousRot, 
                     Vector3 currentPos, Quaternion currentRot) {
    looper = loop;
    previousPosition = previousPos;
    previousRotation = previousRot;
    currentPosition = currentPos;
    currentRotation = currentRot;
  }

  public void Execute() {
    if (looper != null) {
      looper.transform.position = currentPosition;
      looper.transform.rotation = currentRotation;
    }
  }

  public void Undo() {
    if (looper != null) {
      looper.transform.position = previousPosition;
      looper.transform.rotation = previousRotation;
    }
  }
}
