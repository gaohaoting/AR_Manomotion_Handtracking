using UnityEngine;
using System;

[Serializable]
public class BezierPoint : OrientedMovingPoint {

	// CONSTRUCTORS
	public BezierPoint() : base() {}
	public BezierPoint(OrientedMovingPoint op) : base(op) {
		cp0 = position;
		cp1 = position;
		initialRotation = rotation;
	}
    public BezierPoint(BezierPoint other) : base(other)
    {
        cp0 = other.cp0;
        cp1 = other.cp1;
        initialRotation = other.initialRotation;
        createTimeRelativeToSpline = other.createTimeRelativeToSpline;
        widthTarget = other.widthTarget;
        thicknessTarget = other.thicknessTarget;
        pointWidthSettings = new PointWidthSettings(other.pointWidthSettings);
    }

	public float inclinmation {
		get {
			float dot = Vector3.Dot (rotation * Vector3.forward, initialRotation * Vector3.forward);
			return 1f - Mathf.Abs (dot);
		}
	}
		
	public float createTimeRelativeToSpline = -1f;
	public Quaternion initialRotation; // Wand rotation
	public float initialWidthPostClampScaleFactor = 1f;
	public float initialCustomParameter = 0f;
	protected float _widthTarget = 1f;
	public float widthTarget {
		set{
			_widthTarget = value;
			widthIsAnimating = true;
		}
		get {
			return _widthTarget;
		}
    }
    protected float _thicknessTarget = 1f;
    public float thicknessTarget
    {
        set
        {
            _thicknessTarget = value;
        }
        get
        {
            return _thicknessTarget;
        }
    }

    [Serializable]
    public class PointWidthSettings
    {
        public PointWidthSettings() { }
        public PointWidthSettings(PointWidthSettings other)
        {
            widthAnimationTime = other.widthAnimationTime;
            fixedThickness = other.fixedThickness;
            fixedWidth = other.fixedWidth;
            useFixedWidth = other.useFixedWidth;
            thicknessFollowsWidth = other.thicknessFollowsWidth;
            minWidth = other.minWidth;
            maxWidth = other.maxWidth;
            maxWidthChangePct = other.maxWidthChangePct;
            speedAffectWidthFactor = other.speedAffectWidthFactor;
            maxSpeedToAffectWidth = other.maxSpeedToAffectWidth;
            speedAffectWidthRamp = other.speedAffectWidthRamp;
            widthTargetAnimationRamp = other.widthTargetAnimationRamp;
			//customPostClampWidthMultiplier = other.customPostClampWidthMultiplier;
        }
        public float widthAnimationTime = 1f; // seconds
        public float fixedThickness = 0.01f; // centimeters
        public float fixedWidth = 0.1f;
        public bool useFixedWidth = false;
        public bool thicknessFollowsWidth = false;
        public float minWidth = 0.01f;
        public float maxWidth = 0.1f;
        public float maxWidthChangePct = 0f;
        public float speedAffectWidthFactor = 1f;
        public float maxSpeedToAffectWidth = 1.4f;
		public float[] speedAffectWidthRamp = new float[] { 0f, 1f }; // linear
		//public float customPostClampWidthMultiplier = 1f;
        //public float bendAffectWidthFactor = 0f;
        public float[] widthTargetAnimationRamp = new float[] { 0f, 1f }; // linear
    }
    public PointWidthSettings pointWidthSettings = new PointWidthSettings();
    public bool widthIsAnimating = true;
    // Calculates target depending on speed and previous point, time animation done in point.CalcSizeAtTime
    public void CalcWidthAndThicknessTarget(BezierPoint prevPoint = null)
    {
        if (pointWidthSettings == null)
        {
            Debug.LogWarning("Point width settings not set.");
            return;
        }
        float width = 0f;
        if (pointWidthSettings.useFixedWidth)
        {
            width = pointWidthSettings.fixedWidth;
        }
        else
        {
            float speedWidthPct = pointWidthSettings.speedAffectWidthRamp.Sample(Mathf.Clamp01(speed / pointWidthSettings.maxSpeedToAffectWidth));
            //float speedWidthPct = Interpolation.CubicInterpolation (Mathf.Clamp01(point.speed / widthSettings.maxSpeedToAffectWidth), widthSettings.speedAffectWidthRampCubic[0], widthSettings.speedAffectWidthRampCubic[1], widthSettings.speedAffectWidthRampCubic[2], widthSettings.speedAffectWidthRampCubic[3]);
            //float speedWidthPct = Interpolation.LinearInterpolation01 (Mathf.Clamp01(point.speed / maxSpeedToAffectWidth));
            //float inclinationWidthPct = 1f;

            if (pointWidthSettings.speedAffectWidthFactor >= 0f) // faster == thicker
            {
                width = pointWidthSettings.minWidth + (pointWidthSettings.maxWidth - pointWidthSettings.minWidth) * pointWidthSettings.speedAffectWidthFactor * speedWidthPct;
            }
            else
            {
                width = pointWidthSettings.maxWidth + (pointWidthSettings.maxWidth - pointWidthSettings.minWidth) * pointWidthSettings.speedAffectWidthFactor * speedWidthPct;
            }
            if (pointWidthSettings.maxWidthChangePct > 0f && prevPoint != null)
            {
                float prevPointWidth = Mathf.Clamp(prevPoint.widthTarget, pointWidthSettings.minWidth, pointWidthSettings.maxWidth);
                width = Mathf.Clamp(width, prevPointWidth - prevPointWidth * pointWidthSettings.maxWidthChangePct, prevPointWidth + prevPointWidth * pointWidthSettings.maxWidthChangePct);
            }
			width *= initialWidthPostClampScaleFactor;
        }
        //Debug.Log ("width = " + width);
        widthTarget = width;

        thicknessTarget = pointWidthSettings.thicknessFollowsWidth ? widthTarget : pointWidthSettings.fixedThickness;
    }
    public Vector3 CalcSizeAtTime(float timeRelativeToSpline) {
		if (widthIsAnimating && pointWidthSettings.widthAnimationTime > 0f) {
			float timePct = Mathf.Clamp01 ((timeRelativeToSpline - createTimeRelativeToSpline) / pointWidthSettings.widthAnimationTime);
			float mu = 1f;

			// linear
			//mu = Interpolation.LinearInterpolation01(timePct);

			// cos
			//mu = Interpolation.CosineInterpolation01(timePct);

			// inv exponential, quick in, slow out
			//mu = Interpolation.InverseExponentialInterpolation01(timePct);

			// array
			mu = pointWidthSettings.widthTargetAnimationRamp.Sample(timePct);

			widthIsAnimating = timePct < 1f;
			size.x = _widthTarget * mu;
		} else {
			size.x = _widthTarget;
		}

        size.x = Mathf.Max(size.x, 0.0001f);
        size.y = Mathf.Max((pointWidthSettings.thicknessFollowsWidth ? size.x : _thicknessTarget), 0.0001f);
		return size;
	}


    public string GetInfoText()
    {
        return "bendAngle = " + bendAngle + " | bendAngleAcc = " + bendAngleAccumulated + " | bendAccDist = " + bendAngleAccumulationDistance + " | bendAngleAcc / bendAccDist = " + bendAngleAccumulated / bendAngleAccumulationDistance +
            "\nbendDirection * up = " + (Vector3.Dot(bendDirection, rotation * Vector3.up)) +
            "\nflippedUp = " + (flippedUp ? "Yes" : "No") + " | calc from right vec = " + (calculatedRotationFromRightVector ? "Yes" : "No") +
            "\ntime = " + createTimeRelativeToSpline;
    }


    // PROPERTIES
    public Vector3 cp0;
	public Vector3 cp1;

    //public Vector3 rotEuler = Vector3.zero;
    //public float rotRightDeg = 0f;
    public float bendAngle = 0f;
    public Vector3 bendDirection = Vector3.zero;
    public float bendAngleAccumulated = 0f;
    public float bendAngleAccumulationDistance = 0f;
    //public Vector3 bendDirectionAccumulated = Vector3.zero;

    public bool flippedUp = false;
    public bool calculatedRotationFromRightVector = false;

    public bool splitSplineHere = false;
	public bool removeThisPoint = false;
}
