#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

using UnityEngine;
using System.Collections;

// Input module for the Daydream controller.
public class DaydreamInput : InputBase {
  // Button hold time threshold in seconds.
  public float buttonHeldThreshold = 1.0f;

  // Delta threshold for swipe recognition.
  public float minSwipeDelta = 0.2f;

  // Velocity threshold for swipe recognition.
  public float minSwipeVelocity = 4.0f;

  // Next button held recognition time in seconds.
  private float buttonHeldTime;

  // Swipe start position.
  private Vector2 swipeStartPosition;

  // Swipe start time in seconds.
  private float swipeStartTime;

  // Current trigger state.
  private Trigger currentTrigger;

  // Triggers.
  private readonly Trigger[] triggers = {
    Trigger.Build, 
    Trigger.Interact, 
    Trigger.Tweak, 
    Trigger.PlayPause,
    Trigger.Reset,
  };

  void Start() {
    currentTrigger = Trigger.Build;
#if UNITY_EDITOR
    // Lefty. :)
    GvrSettings.Handedness = GvrSettings.UserPrefsHandedness.Left;
#endif
  }

  protected override void Process() {
    if (!(GvrController.ClickButtonDown && GvrController.ClickButtonUp &&
        GvrController.ClickButton)) {
      // Update touchpad swipe state.
      if (GvrController.TouchDown) {
        swipeStartPosition = GvrController.TouchPos;
        swipeStartTime = Time.time;
      } else if (GvrController.TouchUp) {
        Vector2 swipeDeltaPosition = GvrController.TouchPos - swipeStartPosition;
        float swipeDeltaTime = Time.time - swipeStartTime;

        Vector2 swipeVelocity = swipeDeltaPosition / swipeDeltaTime;
        if (Mathf.Abs(swipeDeltaPosition.x) > minSwipeDelta &&
            Mathf.Abs(swipeVelocity.x) > minSwipeVelocity) {
          // Horizontal swipe.
          currentTrigger = (currentTrigger == Trigger.Build) ? Trigger.Interact : Trigger.Build;
        }
      }
    }
    // Update pointer position.
//    inputPointer.localPosition = GvrController.Orientation * Vector3.forward;
    inputPointer.localRotation = GvrController.Orientation;
    // Call base process.
    base.Process();
  }

  protected override Trigger[] GetTriggers() {
    return triggers;
  }

  protected override TriggerState GetTriggerState(Trigger trigger) {
    switch (trigger) {
      case Trigger.Build:
      case Trigger.Interact:
        if (trigger == currentTrigger) {
          if (GvrController.ClickButtonDown) {
            return TriggerState.Pressed;
          } else if (GvrController.ClickButtonUp) {
            return TriggerState.Released;
          } else if (GvrController.ClickButton) {
            return TriggerState.Held;
          }
        }
        break;
      case Trigger.PlayPause:
        if (GvrController.AppButtonUp) {
          return TriggerState.Released;
        } 
        break;
    }
    return TriggerState.None;
  }
}

#endif
