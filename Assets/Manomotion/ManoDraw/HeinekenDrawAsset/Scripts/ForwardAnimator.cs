using UnityEngine;
using System.Collections;

public class ForwardAnimator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float speed = 0.1f;
		transform.position = transform.position + transform.rotation * Vector3.forward * Time.smoothDeltaTime * speed;
	}
}
