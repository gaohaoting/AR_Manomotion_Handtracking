using UnityEngine;
using System.Collections;

public class WandOSC : WandBase {

	public void OnUndoMessage(OscMessage msg) {
		if (sceneManager != null)
			sceneManager.Undo ();
	}
	public void OnUndoAllMessage(OscMessage msg) {
		if (sceneManager != null)
			sceneManager.UndoAll ();
	}
	public void OnRedoMessage(OscMessage msg) {
		if (sceneManager != null)
			sceneManager.Redo ();
	}



	public void OnEndSplineMessage(OscMessage msg) {
		EndCurrentSpline ();
	}
	public void OnSplitSplineMessage(OscMessage msg) {
		if (sceneManager != null) {

			// OSC /split
			//    float timeAbs
			//    int old spline ID
			//    int new spline ID
			//    int index
			int oldSplineId = -1;
			int newSplineId = -1;
			int pointIndex = -1;
			msg.TryGet (1, out oldSplineId);
			msg.TryGet (2, out newSplineId);
			msg.TryGet (3, out pointIndex);

			BezierSpline oldSpline = sceneManager.GetCurrentSpline ();
			if (oldSpline != null && oldSpline.id != oldSplineId) {
				oldSpline = null;
			}
			if (oldSpline == null) {
				oldSpline = sceneManager.GetSplineById (oldSplineId);
			}
			BezierSpline newSpline = sceneManager.SplitSplineAtPointIndex (oldSpline, pointIndex);
			if (newSpline != null) {
				newSpline.id = newSplineId;
				newSpline.remoteCreateTime = oldSpline.remoteCreateTime;
				sceneManager.SetCurrentSpline (newSpline);
			}
		}
	}
	public void OnRemovePointMessage(OscMessage msg) {
		if (sceneManager != null) {

			// OSC /rempt
			//    float timeAbs
			//    int spline ID
			//    int pointIndex
			int splineId = -1;
			int pointIndex = -1;
			msg.TryGet (1, out splineId);
			msg.TryGet (2, out pointIndex);

			BezierSpline spline = sceneManager.GetCurrentSpline ();
			if (spline != null && spline.id != splineId) {
				spline = null;
			}
			if (spline == null) {
				spline = sceneManager.GetSplineById (splineId);
			}
			sceneManager.RemovePointFromSpline (spline, pointIndex);
		}
	}
	public void OnCreateContinuingSplineMessage(OscMessage msg) {
		if (sceneManager != null) {

			// OSC /cont
			//    float timeAbs
			//    int new spline ID
			float time = -1;
			int newSplineId = -1;
			msg.TryGet (0, out time);
			msg.TryGet (1, out newSplineId);

			BezierSpline newSpline = sceneManager.CreateContinuingSpline();
			if (newSpline != null)
			{
				newSpline.id = newSplineId;
				newSpline.remoteCreateTime = time;
			}
		}
	}
	protected void OnAddOrSetPointMessage(OscMessage msg, bool add) {
		if (sceneManager != null) {

			// OSC /addpt
			//    float timeAbs
			//   'Spline' spline data : spline id / brush index / color
			// 	 'Point' point data : postion / rotation / velocity
			//    float width factor
			// 	  float custom param
			float timeAbsolute = -1;
			int splineId = -1;
			int brushIndex = -1;
			Color32 color;
			Vector3 pointPosition;
			Quaternion pointInitialRotation;
			Vector3 pointVelocity;
			float pointWidthPostClampScaleFactor;
			float pointCustomParameter;
			msg.TryGet (0, out timeAbsolute);
			msg.TryGet (1, out splineId);
			msg.TryGet (2, out brushIndex);
			msg.TryGet (3, out color);
			pointPosition = BezierPackageReceiverOSC.ReadVector3FromMessage (msg, 4);
			pointInitialRotation = BezierPackageReceiverOSC.ReadQuatFromMessage (msg, 7);
			pointVelocity = BezierPackageReceiverOSC.ReadVector3FromMessage (msg, 11);
			msg.TryGet (14, out pointWidthPostClampScaleFactor);
			msg.TryGet (15, out pointCustomParameter);

			if (msg.args.Count > 16) {
				float timeScaleFactor = 1f;
				if (msg.TryGet (16, out timeScaleFactor) && sceneManager.timeScaleFactor != timeScaleFactor) {
					sceneManager.timeScaleFactor = timeScaleFactor;
				}
			}

			// add or set last
			BezierSpline spline = sceneManager.GetCurrentSpline();
			if (spline == null)
			{
				spline = sceneManager.GetSplineById(splineId);
				if (spline != null)
				{
					sceneManager.SetCurrentSpline(spline);
					Debug.Log("Setting current spline to id = " + spline.id);
				}
			}
			else if (spline.id != splineId)
			{
				EndCurrentSpline();
				spline = null;
			}
			if (spline == null)
			{
				//Debug.LogWarning("Creating new spline even though remote doesn't say.");
				spline = sceneManager.CreateNewSpline(pointPosition, brushIndex, color, true);
				if (spline == null)
				{
					return;
				}
				spline.id = splineId;
				spline.IsBeingDrawnByWand = true;
				spline.remoteCreateTime = timeAbsolute;
			}
			OrientedMovingPoint op = new OrientedMovingPoint(
				pointPosition,
				pointInitialRotation,
				pointVelocity,
				Vector3.zero
			);

			BezierPoint bp = null;
			if (add) {
				bp = sceneManager.AddPointToCurrentSpline (op, pointWidthPostClampScaleFactor, pointCustomParameter);
			} else {
				bp = sceneManager.SetLastPointOfCurrentSpline(op, pointWidthPostClampScaleFactor, pointCustomParameter);
			}

			if (bp != null)
			{
				bp.createTimeRelativeToSpline = timeAbsolute - spline.remoteCreateTime;
			}
		}
	}
	public void OnAddPointMessage(OscMessage msg) {
		OnAddOrSetPointMessage (msg, true);
	}
	public void OnSetLastPointMessage(OscMessage msg) {
		OnAddOrSetPointMessage (msg, false);
	}
}
