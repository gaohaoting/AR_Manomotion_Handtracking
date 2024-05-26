using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof(OscIn))]
public class BezierPackageReceiverOSC : BezierPackageReceiver {
	
	public OscIn oscIn = null;
	//void Start() {
	//	oscIn = gameObject.GetComponent<OscIn> ();
	//}

	public WandOSC wandOsc = null;

	public override string Status ()
	{
		if (oscIn != null)
			return (oscIn.isOpen ? "Listening to port " : "Not connected to port ") + oscIn.port;
		return "No OscIn connected";
	}

	public static Vector3 ReadVector3FromMessage(OscMessage msg, int offset = 0) {
		Vector3 v;

		msg.TryGet (offset + 0, out v.x);
		msg.TryGet (offset + 1, out v.y);
		msg.TryGet (offset + 2, out v.z);

		return v;
	}
	public static Quaternion ReadQuatFromMessage(OscMessage msg, int offset = 0) {
		Quaternion q;

		msg.TryGet (offset + 0, out q.x);
		msg.TryGet (offset + 1, out q.y);
		msg.TryGet (offset + 2, out q.z);
		msg.TryGet (offset + 3, out q.w);

		return q;
	}
    
	public void OnSetCameraTransformMessage(OscMessage msg)
	{
		if (cameraTransform != null)
		{
			//	  float timeAbs
			//    Vec3 position
			//    Quat rotation
			cameraTransform.position = ReadVector3FromMessage(msg, 1);
			cameraTransform.rotation = ReadQuatFromMessage (msg, 4);
		}
	}
	public void OnSetLeftControllerTransformMessage(OscMessage msg)
	{
		if (leftControllerTransform != null)
		{
			//	  float timeAbs
			//    Vec3 position
			//    Quat rotation
			leftControllerTransform.position = ReadVector3FromMessage(msg, 1);
			leftControllerTransform.rotation = ReadQuatFromMessage (msg, 4);
        }
	}
	public void OnSetRightControllerTransformMessage(OscMessage msg)
	{
		if (rightControllerTransform != null)
		{
			//	  float timeAbs
			//    Vec3 position
			//    Quat rotation
			rightControllerTransform.position = ReadVector3FromMessage(msg, 1);
			rightControllerTransform.rotation = ReadQuatFromMessage (msg, 4);
		}
	}

	public void OnUpdateBrushMessage(OscMessage msg)
	{
		if (settings != null)
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

			int brushIndex = -1;
			msg.TryGet (1, out brushIndex);
			BrushSettings brush = settings.GetBrushById(brushIndex);
			if (brush == null)
			{
				Debug.LogWarning("Trying to set remote brush that does not exist locally (" + brushIndex + ")");
				return;
			}

			msg.TryGet (2, out brush.color);
			msg.TryGet (3, out brush.splinePathSettings.simplifyStraightSectionsBelowAngle);
			msg.TryGet (4, out brush.splinePathSettings.simplifyStraightSectionsBelowWidthDiff);
			msg.TryGet (5, out brush.splineWidthSettings.keepAnimatingAfterSplineEnded);
			msg.TryGet (6, out brush.pointWidthSettings.widthAnimationTime);
			msg.TryGet (7, out brush.pointWidthSettings.minWidth);
			msg.TryGet (8, out brush.pointWidthSettings.maxWidth);

		}
	}

	public void ClearAllDefaultOscMappings() {
		if (oscIn == null)
			return;

		oscIn.Unmap (wandOsc.OnUndoMessage);
		oscIn.Unmap (wandOsc.OnUndoAllMessage);
		oscIn.Unmap (wandOsc.OnRedoMessage);

		oscIn.Unmap (wandOsc.OnEndSplineMessage);
		oscIn.Unmap (wandOsc.OnSplitSplineMessage);
		oscIn.Unmap (wandOsc.OnCreateContinuingSplineMessage);
		oscIn.Unmap (wandOsc.OnRemovePointMessage);
		oscIn.Unmap (wandOsc.OnAddPointMessage);
		oscIn.Unmap (wandOsc.OnSetLastPointMessage);

		oscIn.Unmap (OnSetCameraTransformMessage);
		oscIn.Unmap (OnSetLeftControllerTransformMessage);
		oscIn.Unmap (OnSetRightControllerTransformMessage);

		oscIn.Unmap (OnUpdateBrushMessage);

		if (recorder != null) {
			oscIn.Unmap (recorder.RecordOscMessage);
		}
	}

	public void AddDefaultOscMappings() {
		if (oscIn == null)
			return;

		if (wandOsc != null) {
			oscIn.Map ("/undo", wandOsc.OnUndoMessage);
			oscIn.Map ("/undoall", wandOsc.OnUndoAllMessage);
			oscIn.Map ("/redo", wandOsc.OnRedoMessage);

			oscIn.Map ("/end", wandOsc.OnEndSplineMessage);
			oscIn.Map ("/split", wandOsc.OnSplitSplineMessage);
			oscIn.Map ("/cont", wandOsc.OnCreateContinuingSplineMessage);
			oscIn.Map ("/rempt", wandOsc.OnRemovePointMessage);
			oscIn.Map ("/addpt", wandOsc.OnAddPointMessage);
			oscIn.Map ("/setpt", wandOsc.OnSetLastPointMessage);
		}

		oscIn.Map ("/camera", OnSetCameraTransformMessage);
		oscIn.Map ("/leftctrl", OnSetLeftControllerTransformMessage);
		oscIn.Map ("/rightctrl", OnSetRightControllerTransformMessage);

		oscIn.Map ("/brush", OnUpdateBrushMessage);

		if (recorder != null) {
			oscIn.Map ("/undo", recorder.RecordOscMessage);
			oscIn.Map ("/undoall", recorder.RecordOscMessage);
			oscIn.Map ("/redo", recorder.RecordOscMessage);

			oscIn.Map ("/end", recorder.RecordOscMessage);
			oscIn.Map ("/split", recorder.RecordOscMessage);
			oscIn.Map ("/cont", recorder.RecordOscMessage);
			oscIn.Map ("/rempt", recorder.RecordOscMessage);
			oscIn.Map ("/addpt", recorder.RecordOscMessage);
			oscIn.Map ("/setpt", recorder.RecordOscMessage);

			oscIn.Map ("/camera", recorder.RecordOscMessage);
			oscIn.Map ("/leftctrl", recorder.RecordOscMessage);
			oscIn.Map ("/rightctrl", recorder.RecordOscMessage);

			oscIn.Map ("/brush", recorder.RecordOscMessage);
		}
	}
}
