using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

abstract public class BezierPackageReceiver : MonoBehaviour {
	
	public BezierPackageRecorder recorder = null;

	public Transform cameraTransform = null;
    public Transform leftControllerTransform = null;
    public Transform rightControllerTransform = null;
    public Settings settings = null;

	abstract public string Status ();
}
