using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class InputManager : MonoBehaviour {
  // Callback for when a input down event is received.
  public void OnInputDown(InputEventData data) {
    Debug.Log(data.trigger + " is down: " + data.selectedObject);
  }

  // Callback for when a input up event is received.
  public void OnInputUp(InputEventData data) {
    Debug.Log(data.trigger + " is up: " + data.selectedObject);
  }

  // Callback for when a input drag event is received.
  public void OnInputDrag(InputEventData data) {
    Debug.Log(data.trigger + " is dragging: " + data.selectedObject);
  }
}
