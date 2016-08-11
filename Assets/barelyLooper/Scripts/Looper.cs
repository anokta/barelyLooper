﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Class that controls a looper object.
public class Looper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
  // Audio source.
  public GvrAudioSource source;

  // Looper manager instance.
  public LooperManager looperManager;

  // Loop data in samples.
  private float[] data;

  // Object distance to camera.
  private float distance;

  // Fade length in samples for each side of the loop.
  private int fadeLengthSamples;

  // Hit offset from the center point.
  private Vector3 pressOffset;

  // Time threshold in seconds until a double-click can be registered.
  private float validDoubleClickTime;
 
  // Allowed maximum click time in seconds.
  private static float clickTimeThreshold = 0.35f;

  void Awake () {
    distance = transform.position.z;
    fadeLengthSamples = AudioSettings.GetConfiguration().dspBufferSize;
  }

  // Sets the transform of the looper to directly face to the |camera| with given |offset|.
  public void SetTransform (Transform camera, Vector3 offset) {
    Vector3 direction = distance * camera.forward + camera.rotation * offset;
    transform.position = camera.position + direction;
    transform.rotation = Quaternion.LookRotation(-direction,
                                                 Vector3.Cross(camera.right, direction));
  }

  public void SetAudioClip (float[] originalData, double targetLength, int frequency) {
    // Fill in the loop data.
    int lengthSamples = (int)(targetLength * frequency);
    data = new float[lengthSamples];
    for (int i = 0; i < originalData.Length; ++i) {
      data[i % lengthSamples] = originalData[i];
    }
    // Smoothen both ends.
    for (int i = 0; i < Mathf.Min(fadeLengthSamples, lengthSamples / 2); ++i) {
      float fade = 1.0f / Mathf.Sqrt(i + 1);
      data[i] *= fade;
      data[lengthSamples - i - 1] *= fade;
    }
    // Set loop data as the new clip.
    source.clip = AudioClip.Create("Loop", lengthSamples, 1, frequency, false);
    source.clip.SetData(data, 0);
  }

  // Starts loop playback at given dsp |startTime| with |playbackOffsetSamples| offset.
  public void StartPlayback (double startTime, int playbackOffsetSamples) {
    source.timeSamples = playbackOffsetSamples % source.clip.samples;
    source.PlayScheduled(startTime);
  }

  // Returns raw audio data.
  public float[] GetAudioData () {
    return data;
  }

  // Implements |IPointerDownHandler.OnPointerDown| callback.
  public void OnPointerDown (PointerEventData eventData) {
    // Calculate where the trigger was pressed relative to the center of the looper object.
    Transform camera = eventData.pressEventCamera.transform;
    Vector3 rotatedOffset = (transform.position - camera.position) - distance * camera.forward;
    pressOffset = Quaternion.Inverse(camera.rotation) * rotatedOffset;
  }

  // Implements |IPointerUpHandler.OnPointerUp| callback.
  public void OnPointerUp (PointerEventData eventData) {
    if (Time.time < validDoubleClickTime) { // double click
      // Remove the looper.
      looperManager.DestroyLooper(this);
    } else {
      validDoubleClickTime = Time.time + clickTimeThreshold;
    }
  }

  // Implements |IDragHandler.OnDrag| callback.
  public void OnDrag (PointerEventData eventData) {
    // Drag and drop the looper object.
    SetTransform(eventData.pressEventCamera.transform, pressOffset);
  }
}
