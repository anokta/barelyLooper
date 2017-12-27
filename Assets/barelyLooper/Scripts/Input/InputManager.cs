using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class InputManager : MonoBehaviour {
  private GameObject selectedObject = null;

  // Callback for when a input down event is received.
  public void OnInputDown(InputEventData data) {
    Debug.Log(data.trigger + " is down: " + data.selectedObject);
    selectedObject = data.selectedObject;
  }

  // Callback for when a input up event is received.
  public void OnInputUp(InputEventData data) {
    Debug.Log(data.trigger + " is up: " + data.selectedObject);
    selectedObject = null;
  }

  // Callback for when a input drag event is received.
  public void OnInputDrag(InputEventData data) {
    Debug.Log(data.trigger + " is dragging: " + data.selectedObject);
    if (selectedObject != null) {
      selectedObject.transform.position = data.position + data.offset;
    }
  }
}
