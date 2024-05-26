using UnityEngine;
using System.Collections;

public abstract class WandBase : MonoBehaviour {

	// SCENE MANAGER
	public BezierSplineSceneManager sceneManager = null;


	protected BezierSpline StartNewSplineWithPoint(OrientedMovingPoint point, float widthPostClampScaleFactor = 1f, float customParameter = 0f) {
		if (sceneManager == null) {
			Debug.LogError ("No SceneManager to create new splines.");
			return null;
		}

		BezierSpline spline = sceneManager.CreateNewSpline (point.position, selectedBrush, new Color32(), false);
		sceneManager.AddPointToCurrentSpline(point, widthPostClampScaleFactor, customParameter);
		spline.IsBeingDrawnByWand = true;
        return spline;
	}
	protected void EndCurrentSpline() {
		if (sceneManager != null) {
            BezierSpline spline = sceneManager.GetCurrentSpline();
            if (spline != null)
            {
                spline.IsBeingDrawnByWand = false;
            }
			sceneManager.EndCurrentSpline ();
		}
	}
    
    


	[SerializeField]
	protected float _minSampleDistance = 1f;
	protected float _minSampleDistanceSquared;
	public float minSampleDistance {
		protected set {
			_minSampleDistance = value;
			_minSampleDistanceSquared = value * value;
		}
		get {
			return _minSampleDistance;
		}
	}

	public int selectedBrush = -1;


	// MONO DEVELOP
	protected void Start() {
		minSampleDistance = _minSampleDistance;
		if (sceneManager == null) {
			Debug.LogWarning ("No SceneManager connected to wand.");
		}
	}
}
