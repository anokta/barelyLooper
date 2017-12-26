using UnityEngine;
using System.Collections;

public class RecordController : MonoBehaviour {

  private float distance;

  void Awake() {
    distance = transform.position.z;
  }

  public void SetTransform(Transform camera) {
    Vector3 direction = distance * camera.forward;
    transform.position = camera.position + direction;
    transform.rotation = Quaternion.LookRotation(-direction,
                                                 Vector3.Cross(camera.right, direction));
  }

  public void Activate() {
    gameObject.SetActive(true);
  }

  public void Deactivate() {
    gameObject.SetActive(false);
  }
}
