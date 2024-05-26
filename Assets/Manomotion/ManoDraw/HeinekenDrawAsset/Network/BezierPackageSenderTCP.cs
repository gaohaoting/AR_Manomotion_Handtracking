using UnityEngine;
using System.Collections.Generic;

// TODO: ThreadedEncodeData(package)?

public class BezierPackageSenderTCP : BezierPackageSender {
	
	public TCPSender networkSender = new TCPSender();

	protected Queue<BezierPackage> messageQueue = new Queue<BezierPackage> ();

	private BezierPackage package = new BezierPackage();

	private void SetPackageFromSpline(BezierSpline spline) {
		package.splineId = spline.id;
		package.brushIndex = spline.brushIndex;
		package.color = spline.color;
	}

	private void SetPackageFromPoint(OrientedMovingPoint point) {
		package.pointPosition = point.position;
		package.pointInitialRotation = point.rotation;
		package.pointVelocity = point.velocity;
		//package.pointSpeed = point.speed;
	}

	/*public void EnqueueCreateNewSpline(BezierSpline spline, OrientedMovingPoint point, float createTime) {
		package = new BezierUDPPointPackage ();

		SetPackageFromSpline (spline);
		SetPackageFromPoint (point);

		package.timeAbsolute = createTime;
		package.action = BezierUDPPackageAction.kPackageActionCreateNewSpline;

		messageQueue.Enqueue (package);
    }*/

	public override void EnqueueSplitSplineAtIndex(BezierSpline oldSpline, BezierSpline newSpline, int index, float createTime)
	{
		package = new BezierPackage();

		package.splineId = oldSpline.id;
		package.brushIndex = newSpline.id;
		package.pointVelocity.x = (float)index;

		package.timeAbsolute = createTime;
		package.action = BezierPackageAction.kPackageActionSplitSpline;

		messageQueue.Enqueue(package);
	}

	public override void EnqueueRemovePointFromSpline(BezierSpline spline, int pointIndex, float createTime) {

		package = new BezierPackage();

		package.splineId = spline.id;
		package.brushIndex = pointIndex;

		package.timeAbsolute = createTime;
		package.action = BezierPackageAction.kPackageActionRemovePointFromSpline;

		messageQueue.Enqueue(package);
	}

	public override void EnqueueCreateContinuingSpline(BezierSpline oldSpline, BezierSpline newSpline, OrientedMovingPoint point, float createTime)
	{
		package = new BezierPackage();

		package.splineId = oldSpline.id;
		package.brushIndex = newSpline.id;

		package.timeAbsolute = createTime;
		package.action = BezierPackageAction.kPackageActionCreateContinuingSpline;

		messageQueue.Enqueue(package);
	}

	public override void EnqueueEndSpline(float time) {
		package = new BezierPackage ();

		package.timeAbsolute = time;
		package.action = BezierPackageAction.kPackageActionEndSpline;

		messageQueue.Enqueue (package);
	}

	public override void EnqueueAddPoint(BezierSpline spline, OrientedMovingPoint point, float createTime, float widthPostClampScaleFactor, float customParameter) {
		package = new BezierPackage ();

		SetPackageFromSpline (spline);
		SetPackageFromPoint (point);
		package.pointWidthPostClampScaleFactor = widthPostClampScaleFactor;
		package.pointCustomParameter = customParameter;

		package.timeAbsolute = createTime;
		package.action = BezierPackageAction.kPackageActionAddPoint;

		messageQueue.Enqueue (package);
	}

	public override void EnqueueSetLastPoint(BezierSpline spline, OrientedMovingPoint point, float createTime, float widthPostClampScaleFactor, float customParameter) {
		package = new BezierPackage ();

		SetPackageFromSpline (spline);
		SetPackageFromPoint (point);
		package.pointWidthPostClampScaleFactor = widthPostClampScaleFactor;
		package.pointCustomParameter = customParameter;

		package.timeAbsolute = createTime;
		package.action = BezierPackageAction.kPackageActionSetLastPoint;

		messageQueue.Enqueue (package);
	}

	public override void EnqueueUndo(float time) {
		package = new BezierPackage ();

		package.timeAbsolute = time;
		package.action = BezierPackageAction.kPackageActionUndo;

		messageQueue.Enqueue (package);
	}

	public override void EnqueueUndoAll(float time) {
		package = new BezierPackage ();

		package.timeAbsolute = time;
		package.action = BezierPackageAction.kPackageActionUndoAll;

		messageQueue.Enqueue (package);
	}

	public override void EnqueueRedo(float time) {
		package = new BezierPackage ();

		package.timeAbsolute = time;
		package.action = BezierPackageAction.kPackageActionRedo;

		messageQueue.Enqueue (package);
	}

	protected override void EnqueueTransformOfType(Transform transform, BezierPackageAction action)
	{
		package = new BezierPackage();

		package.timeAbsolute = Time.time;
		package.action = action;

		package.pointPosition = transform.position;
		package.pointInitialRotation = transform.rotation;

		messageQueue.Enqueue(package);
	}

	protected override void EnqueueBrushUpdate(BrushSettings brush, int brushIndex)
	{
		package = new BezierPackage();

		package.timeAbsolute = Time.time;
		package.action = BezierPackageAction.kPackageBrushUpdate;

		package.brushIndex = brushIndex;
		package.color = brush.color;

		package.pointPosition.x = brush.splinePathSettings.simplifyStraightSectionsBelowAngle;
		package.pointPosition.y = brush.splinePathSettings.simplifyStraightSectionsBelowWidthDiff;
		package.pointPosition.z = brush.splineWidthSettings.keepAnimatingAfterSplineEnded ? 1f : 0f;
		package.pointInitialRotation.x = brush.pointWidthSettings.widthAnimationTime;
		//package.pointInitialRotation.y = brush.pointWidthSettings.customPostClampWidthMultiplier;
		package.pointInitialRotation.z = brush.pointWidthSettings.minWidth;
		package.pointInitialRotation.w = brush.pointWidthSettings.maxWidth;

		messageQueue.Enqueue(package);
	}

	public void EnqueueTimeScaleFactor(float timeScaleFactor, float time) {
		package = new BezierPackage ();

		package.timeAbsolute = time;
		package.pointCustomParameter = timeScaleFactor;
		package.action = BezierPackageAction.kPackageTimeScaleFactor;

		messageQueue.Enqueue (package);
	}

	public void EnqueuePackage(BezierPackage package) 
	{
		if (package != null)
			messageQueue.Enqueue (package);
	}

	public override void Connect() {
		if (networkSender != null)
			networkSender.Connect (ip, port);
	}

	public override void Disconnect() {
		if (networkSender != null)
			networkSender.Disconnect ();
	}

	public override string Status ()
	{
		if (networkSender != null)
			return networkSender.Status;
		return "Sender object not initialized";
	}

	public override bool IsConnected() {
		if (networkSender != null)
			return networkSender.IsConnected;
		return false;
	}

	protected override void Send() {
		// Do in thread? Mutex message queue
		while (messageQueue.Count > 0) {
			BezierPackage package = messageQueue.Dequeue ();
			byte[] bytes = package.Serialize ();
			if (networkSender != null) {
				networkSender.Send (bytes);
			}
			if (recorder != null) {
				recorder.RecordBytes (bytes);
			}
			simpleSendCounter++;
		}
	}
}
