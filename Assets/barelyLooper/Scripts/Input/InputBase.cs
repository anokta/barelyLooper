using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

// Input module for the mouse/keyboard controller.
public abstract class InputBase : MonoBehaviour {
  // Pointer raycast mode.
  public enum RaycastMode {
    // Casts a ray from the camera through the target of the pointer.
    Camera,
    // Casts a ray directly from the pointer.
    Direct,
  }

  // Input camera.
  public Camera inputCamera;

  // Input pointer.
  public Transform inputPointer;

  // Input events layer mask.
  public LayerMask inputMask;

  // Input event listener.
  public InputManager inputManager;

  // Pointer raycast mode.
  public RaycastMode raycastMode = RaycastMode.Direct;

  // Entity placement offset from the pointer direction.
  public float placementDistance = 1.0f;

  // Max raycast distance to interact with entities.
  public float maxRaycastDistance = 100.0f;

  // Delta position threshold to register dragging.
  public float minDragDeltaPosition = 0.0f;

  // Delta rotation threshold to register dragging.
  public float minDragDeltaRotation = 0.0f;

  // Time threshold to register dragging.
  public float minDragTime = 0.0f;

  // Trigger state.
  protected enum TriggerState {
    Pressed,
    Released,
    Held,
    None,
  }

  // Input triggers.
  private Trigger[] triggers = null;

  // Input event data for each trigger.
  private InputEventData[] triggerEventData = null;

  // Current raycast hit.
  private RaycastHit hit;

  private GameObject hitObject;

  // Denotes if the current raycast hit selceted an object.
  private bool objectSelected;

  void OnEnable() {
    // Get input info.
    triggers = GetTriggers();
    // Initialize trigger event data.
    triggerEventData = new InputEventData[triggers.Length];
    for (int i = 0; i < triggerEventData.Length; ++i) {
      triggerEventData[i] = new InputEventData();
      triggerEventData[i].pointer = inputPointer;
      triggerEventData[i].trigger = (Trigger) triggers.GetValue(i);
    }
  }

  void Update() {
    Process();
  }

  protected virtual void Process() {
    // Shoot a new raycast.
    CastRay();
    // Update trigger events.
    for (int index = 0; index < triggerEventData.Length; ++index) {
      TriggerState state = GetTriggerState(triggers[index]);
      if (state != TriggerState.None) {
        TriggerEvent(state, triggerEventData[index]);
      }
    }
  }

  // Returns input triggers.
  protected abstract Trigger[] GetTriggers();

  // Returns the current state of given |trigger|.
  protected abstract TriggerState GetTriggerState(Trigger trigger);

  private void CastRay() {
    Vector3 rayPosition = Vector3.zero;
    Vector3 rayDirection = Vector3.zero;
    switch (raycastMode) {
      case RaycastMode.Camera:
        Vector3 placementPosition = inputPointer.position + inputPointer.forward * placementDistance;
        rayPosition = inputCamera.transform.position;
        rayDirection = (placementPosition - rayPosition).normalized;
        break;
      case RaycastMode.Direct:
        rayPosition = inputPointer.position;
        rayDirection = inputPointer.forward;
        break;
    }
    objectSelected = Physics.Raycast(rayPosition, rayDirection, out hit, maxRaycastDistance,
                                     inputMask.value);
  }

  // Triggers given |state| on |inputManager| with populated |data|.
  private void TriggerEvent(TriggerState state, InputEventData data) {
    // Update event data.
    data.selectedObject = objectSelected ? hit.transform.gameObject : null;
    switch (state) {
      case TriggerState.Pressed:
      // Compute press properties.
        if (objectSelected) {
          data.pressPosition = hit.point; 
          data.pressDistance = hit.distance;
          data.pressOffset = data.selectedObject.transform.position - data.pressPosition;
        } else {
          data.pressPosition = WorldFromPointerDirection(placementDistance);
          data.pressDistance = placementDistance;
          data.pressOffset = Vector3.zero;
        }
        data.pressTime = Time.unscaledTime;
        data.pressRotation = data.pointer.rotation;
        data.position = WorldFromPointerDirection(data.pressDistance);
        data.rotation = data.pressRotation;
        data.offset = data.pressOffset;
        data.deltaPosition = Vector3.zero;
        data.deltaRotation = Quaternion.identity;
        data.dragging = false;
        // Send trigger down callback.
        inputManager.OnInputDown(data);
        break;
      case TriggerState.Held:
        // Update drag information.
        Vector3 newPosition = WorldFromPointerDirection(data.pressDistance);
        data.deltaPosition = newPosition - data.position;
        Quaternion newRotation = data.pointer.rotation;
        data.deltaRotation = newRotation * Quaternion.Inverse(data.rotation);
        if (data.deltaPosition.sqrMagnitude > 0.0f ||
            data.deltaRotation.eulerAngles.sqrMagnitude > 0.0f) {
          if (!data.dragging && Time.unscaledTime - data.pressTime > minDragTime &&
              Vector3.Distance(newPosition, data.pressPosition) > minDragDeltaPosition &&
              Quaternion.Angle(newRotation, data.pressRotation) > minDragDeltaRotation) {
            data.dragging = true;
          }
          data.position = newPosition;
          data.rotation = newRotation;
          data.offset = data.rotation * Quaternion.Inverse(data.pressRotation) * data.pressOffset;
          // Send trigger drag callback.
          inputManager.OnInputDrag(data);
        }
        break;
      case TriggerState.Released:
        // Send trigger up callback.
        inputManager.OnInputUp(data);
        break;
    }
  }

  // Converts pointer direction to 3D coordinates in world space.
  private Vector3 WorldFromPointerDirection(float distance) {
    switch (raycastMode) {
      case RaycastMode.Camera:
        Vector3 cameraPosition = inputCamera.transform.position;
        Vector3 placementPosition = 
          inputPointer.position + inputPointer.forward * placementDistance;
        return cameraPosition + (placementPosition - cameraPosition).normalized * distance;
      case RaycastMode.Direct:
        return inputPointer.position + inputPointer.forward * distance;
    }
    return Vector3.zero;
  }
}