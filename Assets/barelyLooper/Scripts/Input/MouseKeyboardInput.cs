using UnityEngine;
using System.Collections;

// Input module for mouse and keyboard.
public class MouseKeyboardInput : InputBase {
  // Triggers.
  private readonly Trigger[] triggers = {
    Trigger.Build, 
    Trigger.Interact, 
    Trigger.MiddleMouseButton, 
  };

  protected override Trigger[] GetTriggers() {
    return triggers;
  }

  protected override TriggerState GetTriggerState(Trigger trigger) {
    // Get trigger state for mouse events.
    int mouseId = -1;
    switch (trigger) {
      case Trigger.Build:
        mouseId = 0;
        break;
      case Trigger.Interact:
        mouseId = 1;
        break;
      case Trigger.MiddleMouseButton:
        mouseId = 2;
        break;
    }
    if (mouseId != -1) {
      if (Input.GetMouseButtonDown(mouseId)) { 
        return TriggerState.Pressed;
      } else if (Input.GetMouseButtonUp(mouseId)) { 
        return TriggerState.Released;
      } else if (Input.GetMouseButton(mouseId)) { 
        return TriggerState.Held;
      }
    }
    return TriggerState.None;
  }
}
