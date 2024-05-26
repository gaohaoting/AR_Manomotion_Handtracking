using UnityEngine;
using System.Collections;

public class WandPackage : WandBase {

	// udp receiver delegate
	public void OnReceivePackage(BezierPackage package) {
		if (sceneManager == null) {
			return;
		}

		if (package.action == BezierPackageAction.kPackageTimeScaleFactor)
		{
			sceneManager.timeScaleFactor = package.pointCustomParameter;
			return;
		}
        if (package.action == BezierPackageAction.kPackageActionEndSpline)
        {
            EndCurrentSpline();
            return;
        }
        if (package.action == BezierPackageAction.kPackageActionUndo)
        {
            sceneManager.Undo();
            return;
        }
        if (package.action == BezierPackageAction.kPackageActionUndoAll)
        {
            sceneManager.UndoAll();
            return;
        }
        if (package.action == BezierPackageAction.kPackageActionRedo)
        {
            sceneManager.Redo();
            return;
        }
        if (package.action == BezierPackageAction.kPackageActionSplitSpline)
        {
            BezierSpline oldSpline = sceneManager.GetCurrentSpline();
			if (oldSpline != null && oldSpline.id != package.splineId) {
				oldSpline = null;
			}
            if (oldSpline == null)
            {
                oldSpline = sceneManager.GetSplineById(package.splineId);
            }
            BezierSpline newSpline = sceneManager.SplitSplineAtPointIndex(oldSpline, (int)package.pointVelocity.x);
            if (newSpline != null)
            {
                newSpline.id = package.brushIndex;
                newSpline.remoteCreateTime = oldSpline.remoteCreateTime;
                sceneManager.SetCurrentSpline(newSpline);
            }
            return;
		}
		if (package.action == BezierPackageAction.kPackageActionRemovePointFromSpline)
		{
			BezierSpline spline = sceneManager.GetCurrentSpline();
			if (spline != null && spline.id != package.splineId) {
				spline = null;
			}
			if (spline == null)
			{
				spline = sceneManager.GetSplineById(package.splineId);
			}
			sceneManager.RemovePointFromSpline (spline, package.brushIndex);
			return;
		}
        if (package.action == BezierPackageAction.kPackageActionCreateContinuingSpline)
        {
            BezierSpline newSpline = sceneManager.CreateContinuingSpline();
            if (newSpline != null)
            {
                newSpline.id = package.brushIndex;
                newSpline.remoteCreateTime = package.timeAbsolute;
            }
            return;
        }
        /*if (package.action == BezierUDPPackageAction.kPackageActionCreateNewSpline)
        {
            BezierSpline newSpline = sceneManager.CreateNewSpline(package.pointPosition, package.brushIndex, package.color, true);
            if (newSpline != null)
            {
                newSpline.id = package.splineId;
                newSpline.remoteCreateTime = package.timeAbsolute;
            }
            return;
        }*/

        if (package.action == BezierPackageAction.kPackageActionAddPoint ||
            package.action == BezierPackageAction.kPackageActionSetLastPoint)
        {
            // add or set last
            BezierSpline spline = sceneManager.GetCurrentSpline();
            if (spline == null)
            {
                spline = sceneManager.GetSplineById(package.splineId);
                if (spline != null)
                {
                    sceneManager.SetCurrentSpline(spline);
                    Debug.Log("Setting current spline to id = " + spline.id);
                }
            }
            else if (spline.id != package.splineId)
            {
                EndCurrentSpline();
                spline = null;
            }
            if (spline == null)
            {
                //Debug.LogWarning("Creating new spline even though remote doesn't say.");
                spline = sceneManager.CreateNewSpline(package.pointPosition, package.brushIndex, package.color, true);
                if (spline == null)
                {
                    return;
                }
                spline.id = package.splineId;
                spline.IsBeingDrawnByWand = true;
                spline.remoteCreateTime = package.timeAbsolute;
            }
            OrientedMovingPoint op = new OrientedMovingPoint(
                package.pointPosition,
                package.pointInitialRotation,
                package.pointVelocity,
                Vector3.zero
            );

            BezierPoint bp = null;
            switch (package.action)
            {
                case BezierPackageAction.kPackageActionAddPoint:
					bp = sceneManager.AddPointToCurrentSpline(op, package.pointWidthPostClampScaleFactor, package.pointCustomParameter);
                    break;
                case BezierPackageAction.kPackageActionSetLastPoint:
					bp = sceneManager.SetLastPointOfCurrentSpline(op, package.pointWidthPostClampScaleFactor, package.pointCustomParameter);
                    break;
            }

            if (bp != null)
            {
                bp.createTimeRelativeToSpline = package.timeAbsolute - spline.remoteCreateTime;
            }

            return;
        }

        // else: package type we can't handle ...
	}

	// UNITY
	new void Start () {
		base.Start ();
	}

	void Update () {
		
	}
}
