using UnityEngine;
using System.Collections.Generic;

// TODO: ThreadedEncodeData(package)?

[RequireComponent(typeof(OscOut))]
public class BezierPackageSenderOSC : BezierPackageSender {

	OscOut oscOut = null;
	void Start() {
		oscOut = gameObject.GetComponent<OscOut>();
	}
		
	public static void AddVector3ToMessage(OscMessage msg, Vector3 v) {
		msg.Add (v.x);
		msg.Add (v.y);
		msg.Add (v.z);
	}
	public static void AddQuatToMessage(OscMessage msg, Quaternion q) {
		msg.Add (q.x);
		msg.Add (q.y);
		msg.Add (q.z);
		msg.Add (q.w);
	}
	private void AddPointToMessage(OscMessage msg, OrientedMovingPoint point) {
		AddVector3ToMessage (msg, point.position);
		AddQuatToMessage (msg, point.rotation);
		AddVector3ToMessage (msg, point.velocity);
	}
	private void AddSplineToMessage(OscMessage msg, BezierSpline spline) {
		msg.Add (spline.id);
		msg.Add (spline.brushIndex);
		msg.Add (spline.color);
	}

	public override void EnqueueSplitSplineAtIndex(BezierSpline oldSpline, BezierSpline newSpline, int index, float createTime)
	{
		// OSC /split
		//    float timeAbs
		//    int old spline ID
		//    int new spline ID
		//    int index
		OscMessage msg = new OscMessage("/split");
		msg.Add (createTime);
		msg.Add (oldSpline.id);
		msg.Add (newSpline.id);
		msg.Add (index);

		Send (msg);
	}

	public override void EnqueueRemovePointFromSpline(BezierSpline spline, int pointIndex, float createTime)
	{
		// OSC /rempt
		//    float timeAbs
		//    int spline ID
		//    int pointIndex
		OscMessage msg = new OscMessage("/rempt");
		msg.Add (createTime);
		msg.Add (spline.id);
		msg.Add (pointIndex);

		Send (msg);
	}

	public override void EnqueueCreateContinuingSpline(BezierSpline oldSpline, BezierSpline newSpline, OrientedMovingPoint point, float createTime)
	{
		// OSC /cont
		//    float timeAbs
		//    int new spline ID
		OscMessage msg = new OscMessage("/cont");
		msg.Add (createTime);
		msg.Add (newSpline.id);

		Send (msg);
	}

	public override void EnqueueEndSpline(float time)
	{
		// OSC /end
		//	  float timeAbs

		OscMessage msg = new OscMessage ("/end");
		msg.Add (time);
	
		Send (msg);
	}

	public override void EnqueueAddPoint(BezierSpline spline, OrientedMovingPoint point, float createTime, float widthPostClampScaleFactor, float customParameter)
	{
		// OSC /addpt
		//    float timeAbs
		//   'Spline' spline data
		// 	 'Point' point data
		//    float width factor
		// 	  float custom param
		OscMessage msg = new OscMessage("/addpt");
		msg.Add (createTime);
		AddSplineToMessage (msg, spline);
		AddPointToMessage (msg, point);
		msg.Add (widthPostClampScaleFactor);
		msg.Add (customParameter);

		Send (msg);
	}

	public override void EnqueueSetLastPoint(BezierSpline spline, OrientedMovingPoint point, float createTime, float widthPostClampScaleFactor, float customParameter) 
	{
		// OSC /setpt
		//    float timeAbs
		//   'Spline' spline data
		// 	 'Point' point data
		//    float width factor
		// 	  float custom param
		OscMessage msg = new OscMessage("/setpt");
		msg.Add (createTime);
		AddSplineToMessage (msg, spline);
		AddPointToMessage (msg, point);
		msg.Add (widthPostClampScaleFactor);
		msg.Add (customParameter);

		Send (msg);
	}

	public override void EnqueueUndo(float time)
	{
		// OSC /und1
		//	  float timeAbs

		OscMessage msg = new OscMessage ("/undo");
		msg.Add (time);

		Send (msg);
	}

	public override void EnqueueUndoAll(float time)
	{
		// OSC /unda
		//	  float timeAbs

		OscMessage msg = new OscMessage ("/undoall");
		msg.Add (time);

		Send (msg);
	}

	public override void EnqueueRedo(float time) 
	{
		// OSC /red1
		//	  float timeAbs

		OscMessage msg = new OscMessage ("/redo");
		msg.Add (time);

		Send (msg);
	}

	protected override void EnqueueTransformOfType(Transform transform, BezierPackageAction action)
	{
		// OSC /????
		//	  float timeAbs
		//    Vec3 position
		//    Quat rotation

		OscMessage msg = null;

		if (action == BezierPackageAction.kPackageCameraPosition) {
			msg = new OscMessage ("/camera");
		} else if (action == BezierPackageAction.kPackageLeftControllerPosition) {
			msg = new OscMessage ("/leftctrl");
		} else if (action == BezierPackageAction.kPackageRightControllerPosition) {
			msg = new OscMessage ("/rightctrl");
		}

		if (msg != null) {
			msg.Add (Time.time);
			AddVector3ToMessage (msg, transform.position);
			AddQuatToMessage (msg, transform.rotation);
			Send (msg);
		}
	}

	protected override void EnqueueBrushUpdate(BrushSettings brush, int brushIndex)
	{
		// OSC /brush
		//    float timeAbs
		//    int brush index
		// 	  Color brush color
		//    float spln path sett: simplify below angle
		//    float spln path sett: simplify below width
		//	  bool spln width sett: keep animating
		//    float pt width sett: anim time
		//    float pt width sett: min
		//    float pt width sett: max
		OscMessage msg = new OscMessage("/brush");
		msg.Add (Time.time);
		msg.Add (brushIndex);
		msg.Add (brush.color);

		msg.Add (brush.splinePathSettings.simplifyStraightSectionsBelowAngle);
		msg.Add (brush.splinePathSettings.simplifyStraightSectionsBelowWidthDiff);

		msg.Add (brush.splineWidthSettings.keepAnimatingAfterSplineEnded);

		msg.Add (brush.pointWidthSettings.widthAnimationTime);
		msg.Add (brush.pointWidthSettings.minWidth);
		msg.Add (brush.pointWidthSettings.maxWidth);

		Send (msg);
	}

	public override void Connect() {
		if (oscOut != null)
			oscOut.Open (port, ip);
	}

	public override void Disconnect() {
		if (oscOut != null)
			oscOut.Close ();
	}

	public override string Status ()
	{
		return "";
	}

	public override bool IsConnected() {
		if (oscOut != null)
			return oscOut.isOpen;
		return false;
	}

	protected override void Send()
	{
	}

	protected void Send(OscMessage msg)
	{
		oscOut.Send (msg);
		if (recorder != null)
		{
			recorder.RecordOscMessage (msg);
			//Debug.LogWarning ("BezierPackageSenderOSC does not implement recording!");
		}
	}

}
