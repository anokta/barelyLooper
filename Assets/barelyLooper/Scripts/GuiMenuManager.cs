using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GuiMenuManager : MonoBehaviour {
  public Text playbackText;
  public Toggle fixedLengthToggle;
  public Button halveLengthButton, doubleLengthButton;
  public Toggle recordPathToggle;

  public LooperManager loopManager;
  public GvrReticle reticle;

  void Update () {
    // Update reticle color.
    float alpha = reticle.GetComponent<Renderer>().material.color.a;
    Color reticleColor = recordPathToggle.isOn ? Color.red : Color.black;
    if(playbackText.text.Equals("Resume")) {
      reticleColor = Color.gray;
    }
    reticleColor.a = alpha;
    reticle.GetComponent<Renderer>().material.color = reticleColor;
  }

  public void ClearScene () {
    loopManager.Reset();
  }

  public void TogglePlayback() {
    if(playbackText.text.Equals("Pause")) {
      loopManager.Pause();
      playbackText.text = "Resume";
    } else {
      loopManager.UnPause();
      playbackText.text = "Pause";
    }
  }

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
  }
}
