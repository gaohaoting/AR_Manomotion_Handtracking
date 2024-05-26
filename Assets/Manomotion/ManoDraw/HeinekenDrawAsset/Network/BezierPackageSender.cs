using UnityEngine;
using System.Collections.Generic;


public abstract class BezierPackageSender : MonoBehaviour {

	public string ip = "127.0.0.1";
	public int port = 8000;
	public bool connectOnFirstUpdate = true;
	protected bool _firstUpdate = true;

	public BezierPackageRecorder recorder = null;
    public Transform leftControllerTransform = null;
    public Transform rightControllerTransform = null;
    public Transform cameraTransform = null;
    public Settings settings = null;

	public ulong simpleSendCounter = 0;

	/*public void EnqueueCreateNewSpline(BezierSpline spline, OrientedMovingPoint point, float createTime) {
		package = new BezierUDPPointPackage ();

		SetPackageFromSpline (spline);
		SetPackageFromPoint (point);

		package.timeAbsolute = createTime;
		package.action = BezierUDPPackageAction.kPackageActionCreateNewSpline;

		messageQueue.Enqueue (package);
    }*/

	public abstract void EnqueueSplitSplineAtIndex (BezierSpline oldSpline, BezierSpline newSpline, int index, float createTime);
	public abstract void EnqueueRemovePointFromSpline (BezierSpline spline, int pointIndex, float createTime);
	public abstract void EnqueueCreateContinuingSpline (BezierSpline oldSpline, BezierSpline newSpline, OrientedMovingPoint point, float createTime);
	public abstract void EnqueueEndSpline (float time);
	public abstract void EnqueueAddPoint (BezierSpline spline, OrientedMovingPoint point, float createTime, float widthPostClampScaleFactor, float customParameter);
	public abstract void EnqueueSetLastPoint (BezierSpline spline, OrientedMovingPoint point, float createTime, float widthPostClampScaleFactor, float customParameter);
	public abstract void EnqueueUndo (float time);
	public abstract void EnqueueUndoAll (float time);
	public abstract void EnqueueRedo (float time);
	protected abstract void EnqueueTransformOfType (Transform transform, BezierPackageAction action);
	protected abstract void EnqueueBrushUpdate (BrushSettings brush, int brushIndex);

	public abstract void Connect ();
	public abstract void Disconnect ();
	public abstract string Status ();
	public abstract bool IsConnected ();

	protected abstract void Send ();

	// UNITY

	public void Update() {

		if (_firstUpdate) {
			if (connectOnFirstUpdate) {
				Connect ();
			}
			_firstUpdate = false;
		}

        if (cameraTransform != null)
        {
            EnqueueTransformOfType(cameraTransform, BezierPackageAction.kPackageCameraPosition);
        }
        if (leftControllerTransform != null)
        {
            EnqueueTransformOfType(leftControllerTransform, BezierPackageAction.kPackageLeftControllerPosition);
        }
        if (rightControllerTransform != null)
        {
            EnqueueTransformOfType(rightControllerTransform, BezierPackageAction.kPackageRightControllerPosition);
        }

		if (settings != null && settings.keepTrackOfChangesSelective)
        {
            for (int i = 0; i < settings.BrushCount; ++i)
            {
                BrushSettings brush = settings.GetBrushById(i);
                if (brush.didChange)
                {
                    //Debug.Log("BRUSH CHANGED, SEND");
                    EnqueueBrushUpdate(brush, i);
                    brush.didChange = false;
                }
            }
        }


		Send ();
		/*
        // Do in thread? Mutex message queue
        while (messageQueue.Count > 0) {
			BezierPackage package = messageQueue.Dequeue ();
			byte[] bytes = package.Serialize ();
			if (networkSender != null) {
				networkSender.Send (bytes);
			}
			if (recorder != null) {
				recorder.Record (bytes);
			}
			simpleSendCounter++;
		}
		*/

	}
}
