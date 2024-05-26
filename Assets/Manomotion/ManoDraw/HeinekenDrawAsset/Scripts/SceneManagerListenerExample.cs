using UnityEngine;
using System.Collections;

public class SceneManagerListenerExample : MonoBehaviour {

	public BezierSplineSceneManager sceneManager = null;

	public void OnSplineCreated(BezierSpline spline) {
		Debug.Log ("Spline (id = " + spline.id + ") created by scene manager.");
	}
	public void OnSplineEnded(BezierSpline spline) {
		Debug.Log ("Spline (id = " + spline.id + ") ended by scene manager.");
	}

	public void Start() {
		if (sceneManager != null) {
			sceneManager.AddOnSplineEndedDelegate (OnSplineEnded);
			sceneManager.AddOnSplineCreatedDelegate (OnSplineCreated);
		}
	}
}
