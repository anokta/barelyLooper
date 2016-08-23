using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GuiMenuManager : MonoBehaviour {
  public Toggle fixedLengthToggle;
  public Button halveLengthButton, doubleLengthButton;

  public LooperManager loopManager;

  public void ToggleFixedLength () { 
    loopManager.ToggleFixedLength();

    doubleLengthButton.interactable = fixedLengthToggle.isOn;
    halveLengthButton.interactable = fixedLengthToggle.isOn;
  }

  public void DoubleLength () {
    loopManager.DoubleLength();
  }

  public void HalveLength () {
    loopManager.HalveLength();
  }
}
