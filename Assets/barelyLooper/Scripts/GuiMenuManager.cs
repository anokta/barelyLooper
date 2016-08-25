using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GuiMenuManager : MonoBehaviour {
  public Toggle fixedLengthToggle;
  public Button halveLengthButton, doubleLengthButton;
  public Toggle recordPathToggle;

  public LooperManager loopManager;
  public GvrReticle reticle;

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

  public void ToggleRecordPath () { 
    loopManager.ToggleRecordPath();

    float alpha = reticle.GetComponent<Renderer>().material.color.a;
    Color reticleColor = recordPathToggle.isOn ? Color.red : Color.black;
    reticleColor.a = alpha;
    reticle.GetComponent<Renderer>().material.color = reticleColor;
  }
}
