using UnityEngine;
using System.Collections;

public class CameraAnimation : MonoBehaviour {

	public Transform target;
	public float speed = 1f;

	void Update() {
		transform.LookAt(target);
		transform.Translate(Vector3.right * Time.deltaTime * speed);
	}
}
