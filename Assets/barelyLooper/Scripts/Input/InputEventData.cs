using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

// Class that encapsulates input event data that .
public class InputEventData {
  public InputEventData() {
    pointer = null;
    selectedObject = null;
    dragging = false;
    trigger = Trigger.None;
  }

  // Input pointer.
  public Transform pointer;

  // Denotes if the event is dragging.
  public bool dragging;

  // Current event position.
  public Vector3 position;

  // Current event rotation.
  public Quaternion rotation;

  // Position delta since the last update.
  public Vector3 deltaPosition;

  // Rotation delta since the last update.
  public Quaternion deltaRotation;

  // Press event position.
  public Vector3 pressPosition;

  // Press event rotation.
  public Quaternion pressRotation;

  // Press game object camera distance.
  public float pressDistance;

  // Current game object pointer offset.
  public Vector3 offset;

  // Press game object pointer offset.
  public Vector3 pressOffset;

  // Press start time in seconds.
  public float pressTime;

  // Selected game object.
  public GameObject selectedObject;

  // Event trigger.
  public Trigger trigger;
}

// Input triggers.
public enum Trigger {
  Build,
  Interact,
  Tweak,
  PlayPause,
  Reset,
  // Non-VR Desktop (MouseKeyboardInput).
  MiddleMouseButton,
  // VR Vive (ViveInput).
  ViveTrigger,
  ViveTrackPad,
  ViveMenuButton,
  // None.
  None
}
