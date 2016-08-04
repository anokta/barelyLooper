using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Looper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
  // Looper state.
  public enum State {
    Record,
    Playback
  }

  // Audio source.
  public GvrAudioSource source;

  // Object distance to camera.
  public float distance = 1.0f;

  // Hit offset from the center point.
  private Vector3 pressOffset;

  private float validDoubleClickTime;

  // Loop data in samples.
  private float[] data;

  // Fade length in samples for each side of the loop.
  private static int fadeSampleLength = 1024;

  // Allowed maximum click time in seconds.
  private static float clickTimeThreshold = 0.35f;

  void Awake () {
    source = GetComponent<GvrAudioSource>();
  }

  public void SetTransform (Transform camera, Vector3 offset) {
    Vector3 direction = distance * camera.forward + camera.rotation * offset;
    transform.position = camera.position + direction;
    transform.rotation = Quaternion.LookRotation(-direction,
                                                 Vector3.Cross(camera.right, direction));
  }

  public void TrimAudioClip (double length) {
    // Get the original data.
    float[] originalData = new float[source.clip.samples];
    source.clip.GetData(originalData, 0);
    // Fill in the loop data.
    int lengthSamples = (int)(length * source.clip.frequency);
    data = new float[lengthSamples];
    for (int i = 0; i < lengthSamples; ++i) {
      data[i] = originalData[i];
    }
    // Smoothen both ends.
    for (int i = 0; i < Mathf.Min(fadeSampleLength, lengthSamples / 2); ++i) {
      float fade = 1.0f / Mathf.Sqrt(i + 1);
      data[i] *= fade;
      data[lengthSamples - i - 1] *= fade;
    }
    // Set loop data as the new clip.
    source.clip = AudioClip.Create("Loop", lengthSamples, 1, source.clip.frequency, false);
    source.clip.SetData(data, 0);
  }

  // Starts loop playback at given dsp time.
  public void StartPlayback (double startOffset, int playbackOffset) {
    source.timeSamples = playbackOffset;
    source.PlayScheduled(AudioSettings.dspTime + startOffset);
  }

  public float[] GetAudioData () {
    return data;
  }

  public void OnPointerDown (PointerEventData eventData) {
    Transform camera = eventData.pressEventCamera.transform;
    Vector3 rotatedOffset = (transform.position - camera.position) - distance * camera.forward;
    pressOffset = Quaternion.Inverse(camera.rotation) * rotatedOffset;
  }

  public void OnPointerUp (PointerEventData eventData) {
    if (Time.time < validDoubleClickTime) { // double click
      // Remove the looper.
      Destroy(gameObject);
    } else {
      validDoubleClickTime = Time.time + clickTimeThreshold;
    }
  }

  public void OnDrag (PointerEventData eventData) {
    SetTransform(eventData.pressEventCamera.transform, pressOffset);
  }
}
