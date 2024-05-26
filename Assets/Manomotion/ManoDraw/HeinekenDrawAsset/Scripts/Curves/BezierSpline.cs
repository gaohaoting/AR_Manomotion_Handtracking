using UnityEngine;
using System;
using System.Collections.Generic;


[Serializable]
public class BezierSpline : UniqueMesh {

	// META-PROPERTIES
	public float createTime = 0f;
	public float remoteCreateTime = 0f;
    public float timeScaleFactor = 1f;
	public int id = -1;
	[SerializeField]
	private bool _isBeingDrawnByWand = false;
	public bool IsBeingDrawnByWand {
		set {
			_isBeingDrawnByWand = value;
			SetDirtyMeshFromPoint (PointCount - 2);
		}
		get {
			return _isBeingDrawnByWand;
		}
	}
    public bool needsSplit = false;
    public bool wasSplitAtFront= false;
	public bool wasSplitAtEnd = false;
    public BezierPoint ghostPointFront = null;
    public BezierPoint ghostPointEnd = null;
	public bool hasPointsToRemove = false;

    // BRUSH
    public int brushIndex = -1;
	public void SetBrushIndex(int index) { // for debugging / inspector
		brushIndex = index;

		Settings settings = Settings.GetInstance ();
		BrushSettings brush = settings.GetBrushById (index);
		if (brush == null) {
			brushIndex = -1;
			return;
		}
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer> ();
		if (mr != null) {
			mr.material = brush.brushMaterial;
		}
		color = brush.color;
        splineWidthSettings = brush.splineWidthSettings;
		extrudeShapeSettings = brush.extrudeShapeSettings;
		splinePathSettings = brush.splinePathSettings;
	}
	public Color32 color = new Color32(0,0,0,0);

	[Serializable]
	public class SplineWidthSettings {
		public SplineWidthSettings() {}
		public SplineWidthSettings(SplineWidthSettings other) {
            keepAnimatingAfterSplineEnded = other.keepAnimatingAfterSplineEnded;
        }
        public bool keepAnimatingAfterSplineEnded = false;
    }
	public SplineWidthSettings splineWidthSettings = new SplineWidthSettings();
    public BezierPoint.PointWidthSettings pointWidthSettings = new BezierPoint.PointWidthSettings();

    [SerializeField]
	private ExtrudeShapeSettings _extrudeShapeSettings = new ExtrudeShapeSettings ();
	public ExtrudeShapeSettings extrudeShapeSettings {
		set{
			_extrudeShapeSettings = value;
			_extrudeShape = null;
		}
		get {
			return _extrudeShapeSettings;
		}
	}
	private ExtrudeShape _extrudeShape = null;
	public ExtrudeShape extrudeShape {
		set {
			_extrudeShape = value;
			SetDirtyMeshFromPoint (0);
		}
		get {
			if (_extrudeShape == null) {
				_extrudeShape = ExtrudeShape.CreateFromSettings (_extrudeShapeSettings);
				SetDirtyMeshFromPoint (0);
			}
			return _extrudeShape;
		}
	}


	// GENERAL PROPERTIES
	[SerializeField]
	private bool _isExistingPointsEditable = true;
	public bool IsExistingPointsEditable {
		set {
			_isExistingPointsEditable = value;
		}
		get {
			return _isExistingPointsEditable;
		}
	}

    [Serializable]
    public class SplinePathSettings
    {
        public SplinePathSettings() { }
        public SplinePathSettings(SplinePathSettings other) {
            smoothUpVectorFactor = other.smoothUpVectorFactor;
            flipUpVectorCalculationsActive = other.flipUpVectorCalculationsActive;
            //useInitialRotationDirectly = other.useInitialRotationDirectly;
            controlPointLengthPercent = other.controlPointLengthPercent;
            curveSubSegmentMinLength = other.curveSubSegmentMinLength;
            pointSampleMinDistance = other.pointSampleMinDistance;
            bendSplitsSplines = other.bendSplitsSplines;
            keepSimplifyingAfterSplineEnds = other.keepSimplifyingAfterSplineEnds;
            simplifyStraightSectionsBelowAngle = other.simplifyStraightSectionsBelowAngle;
            simplifyStraightSectionsBelowWidthDiff = other.simplifyStraightSectionsBelowWidthDiff;
            //mirrorControlPoints = other.mirrorControlPoints;
        }
        public float smoothUpVectorFactor = 1f;
        public bool flipUpVectorCalculationsActive = true;
        //public bool useInitialRotationDirectly = false;
        public float controlPointLengthPercent = 0.4f;
        public float pointSampleMinDistance = 0.05f;
        public float curveSubSegmentMinLength = 0.01f;
        public bool bendSplitsSplines = true;
        public bool keepSimplifyingAfterSplineEnds = false;
        public float simplifyStraightSectionsBelowAngle = 0f;
        public float simplifyStraightSectionsBelowWidthDiff = 0f;
        //public bool mirrorControlPoints = false;
    }
    [SerializeField]
    private SplinePathSettings _splinePathSettings = new SplinePathSettings();
    public SplinePathSettings splinePathSettings
    {
        set
        {
            _splinePathSettings = value;
            SetDirtySplineFromPoint(0);
        }
        get
        {
            return _splinePathSettings;
        }
    }
    

	/*
	[SerializeField]
	private bool _mirrorControlPoints = false;
	public bool mirrorControlPoints {
		set {
			Debug.LogError ("Try setting control-points to mirror, deprecated.");
			_mirrorControlPoints = value;
			for (int i = 0; i < points.Count; ++i) {
				points [i].mirrorControlPoint = _mirrorControlPoints;
			}
		}
		get {
			return _mirrorControlPoints;
		}
	}*/

	public bool IsDirty() {
		return _dirtySplineFromPoint >= 0 || _dirtyLengthTableFromPoint >= 0 || _dirtySegmentsFromPoint >= 0 || _dirtyMeshFromPoint >= 0;
	}
	protected bool _isAnimating = true;
	public bool IsAnimating() {
		return _isAnimating;
	}



	// POINTS
	[SerializeField]
	private List<BezierPoint> points = new List<BezierPoint>();

	public int PointCount {
		get {
			return points != null ? points.Count : 0;
		}
	}
	public int CurveCount {
		get {
			return Math.Max (0, PointCount - 1);
		}
	}


    public void CopyPointsFromOther(BezierSpline other, int indexFrom = -1, int indexTo = -1)
    {
        if (other == null)
            return;
        indexFrom = indexFrom < 0 ? 0 : indexFrom;
        indexTo = indexTo < 0 ? other.PointCount-1 : indexTo;
        if (!other.IsValidPointIndex(indexFrom) || !other.IsValidPointIndex(indexTo))
            return;

        points.Clear();
        for (int i = indexFrom; i <= indexTo; ++i)
        {
            points.Add(new BezierPoint(other.GetPoint(i)));
        }
        
        SetDirtySplineFromPoint(0);
    }

    public BezierPoint AddPointLocal(OrientedMovingPoint localPoint) {
		if (points == null) {
			points = new List<BezierPoint> ();
		}
		BezierPoint point = new BezierPoint (localPoint);
		point.createTimeRelativeToSpline = PointCount == 0 ? 0f : Time.time - createTime;
        point.widthTarget = localPoint.size.x;
        point.thicknessTarget = localPoint.size.y;
        point.pointWidthSettings = new BezierPoint.PointWidthSettings( pointWidthSettings );
		points.Add (point);
        _doneSimplifyingStraightSections = false;
		SetDirtySplineFromPoint (points.Count - 3);
		if (_isBeingDrawnByWand) {
			SetDirtyMeshFromPoint (points.Count - 3);
		}
        //Debug.Log("ADD point " + (PointCount - 1));
		return point;
	}
	public BezierPoint AddPointGlobal(OrientedMovingPoint globalPoint) {
		OrientedMovingPoint localPoint = new OrientedMovingPoint(
			transform.InverseTransformPoint (globalPoint.position),
			Quaternion.Inverse (transform.rotation) * globalPoint.rotation,
			Quaternion.Inverse (transform.rotation) * globalPoint.velocity,
			globalPoint.size
		);
		return AddPointLocal (localPoint);
	}
	public void RemovePoint(int index) {
		if (points != null && IsValidPointIndex(index)) {
			points.RemoveAt (index);
			SetDirtySplineFromPoint (index - 1);
		}
	}
    public void RemovePoints(int indexFrom, int indexTo)
    {
        if (points != null )
        {
            if (IsValidPointIndex(indexFrom) && IsValidPointIndex(indexTo))
                points.RemoveRange(indexFrom, indexTo - indexFrom + 1);
        }
        SetDirtySplineFromPoint(indexFrom - 1);
    }

	private bool IsValidPointIndex(int index) {
		return points != null && index >= 0 && index < PointCount;
	}
	private bool IsValidCurveIndex(int index) {
		return points != null && index >= 0 && index < CurveCount;
	}

	public BezierPoint GetPoint(int index) {
		if (IsValidPointIndex (index)) {
			return points [index];
		}
		return null;
	}
	public BezierPoint GetLastPoint() {
		if (points == null || points.Count == 0) {
			return null;
		}
		return points[points.Count - 1];
	}

	public void SetPointLocal (int index, BezierPoint point) {
		if (IsValidPointIndex (index)) {
			points [index] = point;
			SetDirtySplineFromPoint (index);
		}
	}
	public void SetPointPositionLocal (int index, Vector3 position) {
		if (IsValidPointIndex (index)) {
			points[index].position = position;
			SetDirtySplineFromPoint (index);
		}
	}
	public void SetPointRotationLocal (int index, Quaternion rotation) {
		if (IsValidPointIndex (index)) {
			points[index].rotation = rotation;
			SetDirtySplineFromPoint (index);
		}
	}
	public void SetPointInitialRotationLocal (int index, Quaternion rotation) {
		if (IsValidPointIndex (index)) {
			points[index].initialRotation = rotation;
			SetDirtySplineFromPoint (index);
		}
	}
	public void SetPointVelocityLocal (int index, Vector3 velocity) {
		if (IsValidPointIndex (index)) {
			points[index].velocity = velocity;
			SetDirtySplineFromPoint (index);
		}
	}
	/*public void SetPointSpeedLocal (int index, float speed) {
		if (IsValidPointIndex (index)) {
			points[index].speed = speed;
			SetDirtySplineFromPoint (index);
		}
	}*/
	public BezierPoint SetLastPointLocal (OrientedMovingPoint localPoint) {
		BezierPoint point = GetLastPoint ();
		if (point != null) {
			point.createTimeRelativeToSpline = Time.time - createTime;
			point.position = localPoint.position;
			point.rotation = localPoint.rotation;
			point.initialRotation = localPoint.rotation;
			point.velocity = localPoint.velocity;
			//point.speed = (localPoint.speed < 0) ? localPoint.velocity.magnitude : localPoint.speed;
			SetDirtySplineFromPoint (points.Count-1);
		}
		return point;
	}
	//public void SetLastPointGlobal (Vector3 position, Quaternion rotation, Vector3 velocity) {
	public BezierPoint SetLastPointGlobal (OrientedMovingPoint globalPoint) {
		if (PointCount > 0) {
			OrientedMovingPoint localPoint = new OrientedMovingPoint (
				transform.InverseTransformPoint (globalPoint.position),
				Quaternion.Inverse (transform.rotation) * globalPoint.rotation,
				Quaternion.Inverse (transform.rotation) * globalPoint.velocity,
				globalPoint.size
			);
			return SetLastPointLocal (localPoint);
		}
		return null;
	}




	// SPLINE
	private int _dirtySplineFromPoint = 0;
	public void SetDirtySplineFromPoint (int index) {
        index = Math.Max(index, 0);
		_dirtySplineFromPoint = (_dirtySplineFromPoint < 0) ? index : Math.Min (_dirtySplineFromPoint, index);
		/*if (!splineSettings.useInitialRotationDirectly) {
			_dirtySplineFromPoint = Math.Max (0, _dirtySplineFromPoint - 1);
	    }*/
	}

	public void UpdateSplineFromPoint(int index) {

		//Debug.Log ("BezierSpline.UpdateSplineFromPoint( " + index + " )");

		BezierPoint p0, p1, p2;
		p0 = index == 0 ? ghostPointFront : GetPoint (index - 1);
		p1 = GetPoint (index);
		p2 = null;
		int i = index;
		while (p1 != null && p1 != ghostPointEnd) {
			p2 = GetPoint (i + 1);
			if (p2 == null && i == PointCount - 1) {
				p2 = ghostPointEnd;
			}

			/*if (splineSettings.useInitialRotationDirectly)
            {
                p1.rotation = p1.initialRotation;
            } else*/
            {
                p1.rotation = CalcPointRotationFromNeighbors(p0, p1, p2, i);
			}

			// cp0
			if (p0 != null) { // calc cp0
				Vector3 p10 = p0.position - p1.position;
				p1.cp0 = p1.position - splinePathSettings.controlPointLengthPercent * p10.magnitude * (p1.rotation * Vector3.forward);
			} else if (p2 != null) { // mirror cp0 from cp1
				Vector3 p12 = p2.position - p1.position;
				p1.cp0 = p1.position - splinePathSettings.controlPointLengthPercent * p12.magnitude * (p1.rotation * Vector3.forward);
			} else { // set default
				p1.cp0 = p1.position - (p1.rotation * Vector3.forward);
			}

			// cp1
			if (p2 != null) { // calc cp1
				Vector3 p12 = p2.position - p1.position;
				p1.cp1 = p1.position + splinePathSettings.controlPointLengthPercent * p12.magnitude * (p1.rotation * Vector3.forward);
			} else if (p0 != null) { // mirror cp1 from cp0
				Vector3 p10 = p0.position - p1.position;
				p1.cp1 = p1.position + splinePathSettings.controlPointLengthPercent * p10.magnitude * (p1.rotation * Vector3.forward);
			} else { // use default
				p1.cp1 = p1.position + (p1.rotation * Vector3.forward);
			}

			// Width
			if (i == 0 && !wasSplitAtFront && extrudeShapeSettings.beginWithZeroWidth) {
				p1.widthTarget = 0;
                p1.thicknessTarget = 0;
			} else if (i == PointCount - 1 && !wasSplitAtEnd && extrudeShapeSettings.endWithZeroWidth) {
				p1.widthTarget = 0;
                p1.thicknessTarget = 0;
            } else {
				p1.CalcWidthAndThicknessTarget (p0);
            }

            // Bending
            if (p0 != null && p2 != null)
            {
                Vector3 backDir = (p0.position - p1.position).normalized;
                Vector3 forwDir = (p2.position - p1.position).normalized;
                p1.bendDirection = Vector3.Slerp(backDir, forwDir, 0.5f).normalized;
                p1.bendAngle = 180f - Vector3.Angle(backDir, forwDir);
            }
            else
            {
                p1.bendDirection = Vector3.zero;
                p1.bendAngle = 0;
            }
            p1.bendAngleAccumulated = p1.bendAngle;


            p0 = p1;
			p1 = p2;
			++i;
		}

        // Bending, again
        if (true)
        {
            int bendAccumulationPointRangeRadius = 2;
            i = index;
            p1 = GetPoint(i);
            float d;
            while(p1 != null)
            {
                p1.bendAngleAccumulated = p1.bendAngle;
                p1.bendAngleAccumulationDistance = 0f;
                Vector3 bendCrossP1 = Vector3.Cross(p1.rotation * Vector3.forward, p1.bendDirection);
                for (int r = 1; r < bendAccumulationPointRangeRadius; ++r)
                {
                    p0 = GetPoint(i - r);
                    p2 = GetPoint(i + r);
                    // TODO : dot prod not correct ...!
                    if (p0 != null)
                    {
                        //d = Vector3.Dot(p1.bendDirection, p0.bendDirection);
                        Vector3 bendCrossP0 = Vector3.Cross(p0.rotation * Vector3.forward, p0.bendDirection);
                        d = Vector3.Dot(bendCrossP1, bendCrossP0);
                        p1.bendAngleAccumulated += d * p0.bendAngle;
                        p1.bendAngleAccumulationDistance += (float)GetLengthBetweenPoints(i-r, i);
                    }
                    if (p2 != null)
                    {
                        //d = Vector3.Dot(p1.bendDirection, p2.bendDirection);
                        Vector3 bendCrossP2 = Vector3.Cross(p2.rotation * Vector3.forward, p2.bendDirection);
                        d = Vector3.Dot(bendCrossP1, bendCrossP2);
                        p1.bendAngleAccumulated += d * p2.bendAngle;
                        p1.bendAngleAccumulationDistance += (float)GetLengthBetweenPoints(i, i+r);
                    }
                }


                /*if (widthSettings.bendAffectWidthFactor != 0f)
                {
                    // TOFO: Implement ...
                    //d = Mathf.Abs(Vector3.Dot(p1.bendDirection, p1.rotation * Vector3.up));
                }*/

                if (splinePathSettings.bendSplitsSplines)
                {
                    d = Mathf.Abs(Vector3.Dot(p1.bendDirection, p1.rotation * Vector3.up));
                    if ((d > 0.5f && (p1.bendAngle > 160f || p1.bendAngleAccumulated > 180f)) ||
                        (d <= 0.8f && (p1.bendAngle > 50f || p1.bendAngleAccumulated > 60f))) // TODO: need to depend on width & point distance too...
                    {
                        needsSplit = true;
                        p1.splitSplineHere = true;
                    }
                }

                ++i;
                p1 = GetPoint(i);
            }

        }




		SetDirtyLengthTableFromPoint(Math.Max(index - 1, 0));
		_dirtySplineFromPoint = -1;
	}

    private bool _doneSimplifyingStraightSections = false;
    private int SimplifyStraightSections(int maxPointsToRemove = -1)
    {
        int removedPoints = 0;
        int pointsNotRemovedBecauseAnimating = 0;

        // do not remove first, second, last and next last point
        int pointIndex = 2;
        BezierPoint p0, p1, p2;
        while (removedPoints < maxPointsToRemove && pointIndex < PointCount-3)
        {
            p0 = GetPoint(pointIndex - 1);
            p1 = GetPoint(pointIndex);
            p2 = GetPoint(pointIndex + 1);
            if (p1.bendAngle < splinePathSettings.simplifyStraightSectionsBelowAngle && 
                Mathf.Abs(p0.widthTarget - p1.widthTarget) < splinePathSettings.simplifyStraightSectionsBelowWidthDiff &&
                Mathf.Abs(p2.widthTarget - p1.widthTarget) < splinePathSettings.simplifyStraightSectionsBelowWidthDiff)
            {
                if (p0.widthIsAnimating || p1.widthIsAnimating || p2.widthIsAnimating)
                {
                    pointsNotRemovedBecauseAnimating++;
                } else
                {
                    //RemovePoint(pointIndex);
					p1.removeThisPoint = true;
                    removedPoints++;
					hasPointsToRemove = true;
				}
				pointIndex++;
            } else
            {
                pointIndex++;
            }
        }

        _doneSimplifyingStraightSections = (removedPoints == 0) && (pointsNotRemovedBecauseAnimating == 0);
        return removedPoints;
    }

	private Quaternion CalcPointRotationFromNeighbors(BezierPoint prevPoint, BezierPoint thisPoint, BezierPoint nextPoint, int id = 0) {
		if (thisPoint == null) {
			return Quaternion.identity;
        }


        Vector3 forward = thisPoint.initialRotation * Vector3.up; // this should be calculated below ...
        Vector3 up = thisPoint.initialRotation * Vector3.forward;
        Vector3 right = thisPoint.initialRotation * Vector3.right;
        float dotLimit = 0.5f;
        

        // calc forward

        if (prevPoint != null && nextPoint != null) { // all three are ok
			forward = (nextPoint.position - prevPoint.position).normalized;
            float d = Vector3.Dot((prevPoint.position - thisPoint.position).normalized, (nextPoint.position - thisPoint.position).normalized);
			if (d >= 1) { // p0 and p2 in the same direction
				// TODO: Detta kan göras på andra (bättre?) sätt
				if (forward.sqrMagnitude == 0) { // när prev == next
					forward = (thisPoint.position - prevPoint.position).normalized; // forward blir riktningen från tidigare punkt till denna
				}
				// keep forward
			}
		}
		else if (prevPoint != null) { // this and prev are ok
			forward = (thisPoint.position - prevPoint.position).normalized;
			if (forward.sqrMagnitude == 0) { // när this == prev
				forward = prevPoint.rotation * Vector3.forward;
            }
		}
		else if (nextPoint != null) { // this and next are ok
			forward = (nextPoint.position - thisPoint.position).normalized;
			if (forward.sqrMagnitude == 0) { // när this == next
				forward = thisPoint.initialRotation * Vector3.up;
			}
		} else // only this point
        {

        }


        // calc up

        float dot = Vector3.Dot(forward, right);
        if (Mathf.Abs(dot) < dotLimit)
        {
            up = Vector3.Cross(forward, thisPoint.initialRotation * Vector3.right).normalized;
            thisPoint.calculatedRotationFromRightVector = true;
        }
        else
        {
            up = thisPoint.initialRotation * Vector3.forward;
            thisPoint.calculatedRotationFromRightVector = false;
        }

        if (prevPoint != null)
        {
            if (splinePathSettings.flipUpVectorCalculationsActive)
            {
                thisPoint.flippedUp = prevPoint.flippedUp;
                dot = Vector3.Dot((thisPoint.flippedUp ? -up : up), prevPoint.rotation * Vector3.up);
                if (dot < 0f && thisPoint.calculatedRotationFromRightVector != prevPoint.calculatedRotationFromRightVector)
                {
                    thisPoint.flippedUp = !prevPoint.flippedUp;
                }
                if (thisPoint.flippedUp)
                {
                    up = -up;
                }
            }

            up = Vector3.Slerp(up, prevPoint.rotation * Vector3.up, splinePathSettings.smoothUpVectorFactor);
        }

        return Quaternion.LookRotation(forward, up);
	}

	private int _prevCurveIndex = -1;
	private BezierCurve curve;
	public OrientedMovingPoint GetInterpolatedOrientedMovingPoint_0toN(double T) {
		if (PointCount == 0) {
			return new OrientedMovingPoint (Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero);
		}
		if (PointCount == 1) {
			return (OrientedMovingPoint)points [0];
		}

		int c;
		double t;
		if (T >= CurveCount) {
			t = 1.0;
			c = CurveCount - 1;
		} else if (T <= 0.0) {
			c = 0;
			t = 0.0;
		} else {
			c = (int)T;
			t = T - c;
		}
		if (c != _prevCurveIndex) {
			curve = GetCurve (c);
		}
		return curve.GetInterpolatedOrientedMovingPointUnclamped (t);
	}
	/*public OrientedMovingPoint GetInterpolatedOrientedMovingPoint_0to1(double t) {
		lengthTable
		int curve = Math.Min(t * CurveCount, CurveCount-1);
		double T = curve + (t * CurveCount - curve);
		return GetInterpolatedOrientedMovingPoint_0toN (T);
	}*/




	// CURVE
	public BezierCurve GetCurve(int index) {
		if (index < 0 || index >= CurveCount) {
			return null;
		}
		return new BezierCurve (points[index], points[index+1]);
	}




	// SEGMENTS
	//private List<int> segmentsBeforePoint = new List<int> ();
	private List<int> segmentsInCurve = new List<int> ();

	private int _dirtySegmentsFromPoint = 0;
	public void SetDirtySegmentsFromPoint (int index) {
        index = Math.Max(index, 0);
        _dirtySegmentsFromPoint = (_dirtySegmentsFromPoint < 0) ? index : Math.Min(_dirtySegmentsFromPoint, index);
	}

	public int GetSegmentsBeforePoint(int index) {
		if (index <= 0) {
			return 0;
		}
		int segments = 0;
		for (int i = 0; i < index && i < segmentsInCurve.Count; ++i) {
			segments += segmentsInCurve [i];
		}
		return segments;
	}
	/*private int GetSegmentsBetweenPoints(int fromIndex, int toIndex) {
		int segmentCount = 0;
		if (IsValidPointIndex (fromIndex) && IsValidPointIndex(toIndex)) {
			for (int i = fromIndex; i < toIndex; ++i) {
				segmentCount += segmentsInCurve [i];
			}
		}
		return segmentCount;
	}*/
	public int GetSegmentsInCurve(int index) {
		if (index >= 0 && index < segmentsInCurve.Count) {
			return segmentsInCurve [index];
		}
		return 0;
	}

	private void UpdateSegmentsFromPoint(int index) {

		if (segmentsInCurve.Count < CurveCount) {
			index = Math.Min (segmentsInCurve.Count, index);
			while (segmentsInCurve.Count < CurveCount) {
				segmentsInCurve.Add (-1);
			}
		} else while (segmentsInCurve.Count > CurveCount) {
			segmentsInCurve.RemoveAt (segmentsInCurve.Count - 1);
		}

		//Debug.Log ("BezierSpline.UpdateSegmentsFromPoint( " + index + " )");

		int totalSegments = GetSegmentsBeforePoint(index);
		int curveSegments;
		for (int c = index; c < CurveCount; ++c) {
			curveSegments = (int)(curveLengths [c] / splinePathSettings.curveSubSegmentMinLength + 1.0);
			segmentsInCurve[c] = curveSegments;
			totalSegments += curveSegments;
		}

		SetDirtyMeshFromPoint (index);
		_dirtySegmentsFromPoint = -1;
	}



	// LENGTH
	private int _lengthTableSubsampling = 7;
	private int _dirtyLengthTableFromPoint = 0;
	private void SetDirtyLengthTableFromPoint(int index) {
        index = Math.Max(index, 0);
		_dirtyLengthTableFromPoint = (_dirtyLengthTableFromPoint < 0) ? index : Math.Min (_dirtyLengthTableFromPoint, index);
	}
	private int lengthTableSize = 0;
	private double[] lengthTable = null;
	private int _lengthTableExtensionMargin = 1000;
	private void ExtendLengthTableToFitSize (int size) {
		if (lengthTable == null) {
			lengthTable = new double[size + _lengthTableExtensionMargin];
		} else if (size > lengthTable.Length) {
			int missingSize = size - lengthTable.Length;
			double[] oldLengthTable = lengthTable;
			lengthTable = new double[oldLengthTable.Length + missingSize + _lengthTableExtensionMargin];
			Array.Copy (oldLengthTable, lengthTable, lengthTableSize);
		}
	}
	private double[] curveLengths = null;
	private void ExtendCurveLengthsArrayToFitCurves () {
		if (curveLengths == null) {
			curveLengths = new double[CurveCount + _lengthTableExtensionMargin / _lengthTableSubsampling];
		} else if (CurveCount > curveLengths.Length) {
			int missingSize = CurveCount - curveLengths.Length;
			double[] oldCurveLengths = curveLengths;
			curveLengths = new double[oldCurveLengths.Length + missingSize + _lengthTableExtensionMargin / _lengthTableSubsampling];
			Array.Copy (oldCurveLengths, curveLengths, oldCurveLengths.Length);
		}
	}

	private void UpdateLengthTableFromPoint(int index) {
		//Debug.Log ("BezierSpline.UpdateLengthTableFromPoint( " + index + " )");
		index = Math.Max (0, index);

		ExtendLengthTableToFitSize (CurveCount * _lengthTableSubsampling + 1);
		ExtendCurveLengthsArrayToFitCurves ();

		int curvesInOldLengthTable = (lengthTable == null || lengthTableSize < 1) ? 0 : (lengthTableSize - 1) / _lengthTableSubsampling;

		int firstCurve = Math.Min(index, curvesInOldLengthTable);
		double[] curveLengthTable = new double[_lengthTableSubsampling + 1];
		double totalLength = 0f;
		if (firstCurve > 0) {
			totalLength = lengthTable [firstCurve * _lengthTableSubsampling];
			//Debug.Log ("totalLength = " + totalLength);
		}
		for (int c = firstCurve; c < CurveCount; ++c) {
			double curveLength = GetCurve (c).CalcLengthTable (curveLengthTable);
			for (int t = 0; t < _lengthTableSubsampling; ++t) {
				lengthTable [c * _lengthTableSubsampling + t] = totalLength + curveLengthTable [t];
			}
			curveLengths [c] = curveLength;
			totalLength += curveLength;
		}
		lengthTable [CurveCount * _lengthTableSubsampling] = totalLength;
		lengthTableSize = CurveCount * _lengthTableSubsampling + 1;

		SetDirtySegmentsFromPoint (index);
		_dirtyLengthTableFromPoint = -1;
	}

	/*public double GetLength_0to1(double t) {
		if (_recalculateLengthTableFromPoint >= 0) {
			UpdateLengthTableFromPoint (_recalculateLengthTableFromPoint);
		}
		return lengthTable.Sample_0to1 (t, lengthTableSize);
	}*/
	public double GetTotalLength() {
		return GetLength_0toN(PointCount-1);
	}
	public double GetLength_0toN(double T) {
		if (T < 0.0)
			T = 0.0;
		int offset = (int)T * _lengthTableSubsampling;
		double t = T - (int)T;
		if(lengthTableSize <= offset) {
			// ERROR
			return 0;
		}
		double iDouble = offset + t * _lengthTableSubsampling;
		int idLower = (int)iDouble;
		int idUpper = (int)(iDouble+1.0);
		if (idUpper >= lengthTableSize)
			return lengthTable [lengthTableSize - 1];
		iDouble -= idLower;
		return (1.0 - iDouble) * lengthTable [idLower] + (iDouble) * lengthTable [idUpper];
	}

	/*private double GetLengthOfCurve(int index) {
		if (_dirtyLengthTableFromPoint >= 0) {
			UpdateLengthTableFromPoint (_dirtyLengthTableFromPoint);
		}
		return 0;
	}*/

    public double GetLengthBetweenPoints(int p0, int p1)
    {
        /*if (_dirtyLengthTableFromPoint >= 0 && _dirtyLengthTableFromPoint < p1)
        {
            UpdateLengthTableFromPoint(_dirtyLengthTableFromPoint);
        }*/

        if (p1 * _lengthTableSubsampling >= lengthTableSize || p0 == p1 || p0 * _lengthTableSubsampling >= lengthTableSize || p0 < 0 || p1 < 0)
            return 0;

        /*if (p0 == 0)
            return lengthTable[p1 * _lengthTableSubsampling];
        if (p1 == 0)
            return -lengthTable[p0 * _lengthTableSubsampling];*/

        return lengthTable[p1 * _lengthTableSubsampling] - lengthTable[p0 * _lengthTableSubsampling];
    }



	// MESH

	private int _dirtyMeshFromPoint = 0;
	public void SetDirtyMeshFromPoint (int index) {
        index = Math.Max(index, 0);
		_dirtyMeshFromPoint = (_dirtyMeshFromPoint < 0) ? index : Math.Min (_dirtyMeshFromPoint, index);
	}
	private bool _updateFromInspector = false;
	private Vector3[] vertices;
	private Vector3[] vertexNormals;
	private Vector2[] uvs;
	private int[] triangles;
	private Vector3[] triangleNormals;
	private Vector3[] triangleCenters;
	private List<int>[] trianglesConnectedToVertex;
	[SerializeField]
	private float _vCoordOffset = 0f;
	public float VCoordOffset {
		set {
			_vCoordOffset = value;
			SetDirtyMeshFromPoint (0);
		}
		get {
			return _vCoordOffset;
		}
	}
	[SerializeField]
	private float _largestVCoord = 0f;
	public float GetLargestVCoord() {
		if (IsDirty()) {
			Update ();
		}
		return _largestVCoord;
	}

	private int vertexMemorySize = 0;
	private Vertex[] vertexMemory = null;
	private int _vertexMemoryExtensionMargin = 1000;
	private void ExtendVertexMemoryToFitSize (int size) {
		if (vertexMemory == null) {
			vertexMemory = new Vertex[size + _vertexMemoryExtensionMargin];
		} else if (size > vertexMemory.Length) {
			int missingSize = size - vertexMemory.Length;
			Vertex[] oldVertexBuffer = vertexMemory;
			vertexMemory = new Vertex[oldVertexBuffer.Length + missingSize + _vertexMemoryExtensionMargin];
			Array.Copy (oldVertexBuffer, vertexMemory, vertexMemorySize);
		}
	}
	public int VertexCount {
		get {
			return vertexMemorySize;
		}
	}
	public Vector3 GetVertexPosition (int i) {
		if (i < 0 || i >= VertexCount)
			return Vector3.zero;
		return vertices [i];
	}
	public Vector3 GetVertexNormal (int i) {
		if (i < 0 || i >= VertexCount)
			return Vector3.zero;
		return vertexNormals [i];
	}
	public void RecalculateMeshNormalsUnity() {
		mesh.RecalculateNormals ();
		vertexNormals = mesh.normals;
	}
	public Vector3 GetTriangleCenterPosition(int i) {
		if (i < 0 || i >= TriangleCount)
			return Vector3.zero;
		return triangleCenters [i];
	}
	public Vector3 GetTriangleNormal(int i) {
		if (i < 0 || i >= TriangleCount)
			return Vector3.zero;
		return triangleNormals [i];
	}

	private int triangleMemorySize = 0;
	public int TriangleCount {
		get {
			return triangleMemorySize;
		}
	}

    private int GetVerticesBeforeSegment(int segment, bool doubleVertices, int verticesInShape)
    {
        return segment * verticesInShape + (doubleVertices ? Mathf.Max(segment - 1, 0) * verticesInShape : 0);
    }

	private void UpdateMeshFromPoint(int index, bool goBackAndCheckForAnimatingPoints = false) {

		//Debug.Log ("BezierSpline.UpdateMeshFromPoint(" + index + ", " + (goBackAndCheckForAnimatingPoints ? "true" : "false") + ")");	

		ExtrudeShape shape = extrudeShape;

		int pointStillAnimating = PointCount;
		if (shape != null) {

            bool doubleVertices = _extrudeShapeSettings.separateVerticesAlongCurve;
            int totalSegments = GetSegmentsBeforePoint (PointCount - 1);
			int edgeLoops = totalSegments + 1;
			int verticesInShape = shape.vertices.Length;
			int linesInShape = shape.lines.Length / 3;
			int trianglesInEndCap = 0;//_extrudeShapeSettings.drawEndCaps ? (shape.endCapTriangels.Length / 3) : 0;
			int verticesInEndCap = 0;//_extrudeShapeSettings.drawEndCaps ? shape.endCapVertices.Length : 0;
			int vertexCount = edgeLoops * verticesInShape + (doubleVertices ? Mathf.Max(edgeLoops - 2, 0) * verticesInShape : 0) + verticesInEndCap * 2;
			int triangleCount = totalSegments * linesInShape * 2 + trianglesInEndCap * 2;
			vertices = new Vector3[vertexCount];
			vertexNormals = new Vector3[vertexCount];
			uvs = new Vector2[vertexCount];
			triangles = new int[triangleCount * 3];
			triangleNormals = new Vector3[triangleCount];
			triangleCenters = new Vector3[triangleCount];
			trianglesConnectedToVertex = new List<int>[vertexCount];
			for (int v = 0; v < vertexCount; ++v) {
				trianglesConnectedToVertex [v] = new List<int> ();
			}

			// partial update
			if (goBackAndCheckForAnimatingPoints) {
				while (index > 0 && points [index].widthIsAnimating) {
					--index;
				}
			}
			int firstVertexIndexToUpdate = 0;
			if (index > 0) {
				firstVertexIndexToUpdate = Math.Min ( GetVerticesBeforeSegment(GetSegmentsBeforePoint (index), doubleVertices, verticesInShape) + verticesInEndCap, vertexMemorySize - verticesInShape - verticesInEndCap); // maybe one too much if index > 0?
			}
			//Debug.Log ("BezierSpline.UpdateMeshFromPoint(" + index + ") | firstVertexIndexToUpdate = " + firstVertexIndexToUpdate);
			//Math.Min(GetSegmentsBeforePoint(index) * verticesInShape, vertexMemorySize - verticesInShape); // maybe one too much if index > 0?
			//Debug.Log("firstVertexIndexToUpdate = " + firstVertexIndexToUpdate + " | edgeLoops = " + edgeLoops + " | vertexCount = " + vertexCount + " | vertexMemorySize = " + vertexMemorySize);
			ExtendVertexMemoryToFitSize (vertexCount);

			OrientedMovingPoint orientedPoint = new OrientedMovingPoint();
			float invUSpan = 1f / shape.uSpan;
			int segmentsBeforeCurve = 0;
			Vector2 edgeLoopScale = new Vector2(1,1);
			int verticesAdded = 0;
			int trianglesAdded = 0;
			int ti;

			float timeRelativeToSpline = _updateFromInspector ? 100f : (Time.time - createTime) * timeScaleFactor;
			BezierPoint bezierPoint = GetPoint (index);
			if (bezierPoint != null) {
				bezierPoint.CalcSizeAtTime (timeRelativeToSpline);
				pointStillAnimating = bezierPoint.widthIsAnimating ? Math.Min (pointStillAnimating, index) : pointStillAnimating;
			}

            // end cap before
            /*
			if (_extrudeShapeSettings.drawEndCaps) {
				BezierPoint firstPoint = GetPoint (0);
				if (firstPoint != null) {
					Quaternion rotate180 = Quaternion.Euler (new Vector3 (0, 180, 0));
					orientedPoint = (OrientedMovingPoint)firstPoint;
					edgeLoopScale.x = orientedPoint.size.x;
					edgeLoopScale.y = orientedPoint.size.y;
					for (int j = 0; j < verticesInEndCap; ++j) {
						int vertexIndex = verticesAdded + j;
						if (vertexIndex < firstVertexIndexToUpdate) {
							Vertex v = vertexMemory [vertexIndex];
							vertices [vertexIndex] = v.vertex;
							vertexNormals [vertexIndex] = v.normal;
							uvs [vertexIndex] = v.uv;
						} else {
							Vertex v = new Vertex ();
							vertices [vertexIndex] = v.vertex = orientedPoint.LocalToWorld (rotate180 * new Vector3(shape.endCapVertices [j].position.x * edgeLoopScale.x, shape.endCapVertices [j].position.y * edgeLoopScale.y, shape.endCapVertices [j].position.z));
							vertexNormals [vertexIndex] = v.normal = orientedPoint.LocalToWorldDirection (rotate180 * shape.endCapVertices [j].normal);
							uvs [vertexIndex] = v.uv = shape.endCapVertices [j].uv;
							vertexMemory [vertexIndex] = v;
						}
					}
					for (int l = 0; l < trianglesInEndCap; ++l) {
						ti = (trianglesAdded + l) * 3;
						triangles [ti] = shape.endCapTriangels [l*3 + 0];
						triangles [ti + 1] = shape.endCapTriangels [l*3 + 1];
						triangles [ti + 2] = shape.endCapTriangels [l*3 + 2];
					}
					verticesAdded += verticesInEndCap;
					trianglesAdded += trianglesInEndCap;
				}
			}
			*/

            // edge loops
            if (doubleVertices)
            {
                OrientedMovingPoint p0, p1 = null;
                for (int curveIndex = 0; curveIndex < CurveCount; ++curveIndex)
                {
                    int segmentsInCurve = GetSegmentsInCurve(curveIndex);
                    if (curveIndex + 1 > index)
                    { // do not update the point we already updated before ...
                        bezierPoint = GetPoint(curveIndex + 1);
                        bezierPoint.CalcSizeAtTime(timeRelativeToSpline);
                        pointStillAnimating = bezierPoint.widthIsAnimating ? Math.Min(pointStillAnimating, curveIndex + 1) : pointStillAnimating;
                    }

                    for (int s = 0; s < segmentsInCurve; ++s)
                    {
                        int vertexOffset = (segmentsBeforeCurve + s) * verticesInShape * 2 + verticesAdded; //GetVerticesBeforeSegment(segmentsBeforeCurve + s, doubleVertices, verticesInShape) + verticesAdded;
                        double T0 = curveIndex + s / (double)(segmentsInCurve);
                        double T1 = curveIndex + (s+1) / (double)(segmentsInCurve);
                        if (p1 != null)
                            p0 = p1;
                        else 
                            p0 = GetInterpolatedOrientedMovingPoint_0toN(T0);
                        p1 = GetInterpolatedOrientedMovingPoint_0toN(T1);
                        for (int j = 0; j < verticesInShape; j++)
                        {
                            int vertexIndex = vertexOffset + j;
                            if (vertexIndex < firstVertexIndexToUpdate)
                            {
                                Vertex v = vertexMemory[vertexIndex];
                                vertices[vertexIndex] = v.vertex;
                                vertexNormals[vertexIndex] = v.normal;
                                uvs[vertexIndex] = v.uv;
                            }
                            else
                            {
                                edgeLoopScale.x = p0.size.x;
                                edgeLoopScale.y = p0.size.y;
                                Vertex v = new Vertex();
                                vertices[vertexIndex] = v.vertex = p0.LocalToWorld(new Vector3(shape.vertices[j].position.x * edgeLoopScale.x, shape.vertices[j].position.y * edgeLoopScale.y, shape.vertices[j].position.z));
                                //vertexNormals[vertexIndex] = v.normal = orientedPoint.LocalToWorldDirection(new Vector3(shape.vertices[j].normal.x * edgeLoopScale.y, shape.vertices[j].normal.y * edgeLoopScale.x, shape.vertices[j].normal.z).normalized);
                                _largestVCoord = _vCoordOffset + (float)(GetLength_0toN(T0) * invUSpan);
                                uvs[vertexIndex] = v.uv = new Vector2(shape.vertices[j].uv.x, _largestVCoord);
                                vertexMemory[vertexIndex] = v;
                            }
                        }
                        //vertexOffset += verticesInShape;
                        for (int j = 0; j < verticesInShape; j++)
                        {
                            int vertexIndex = vertexOffset + verticesInShape + j;
                            if (vertexIndex < firstVertexIndexToUpdate)
                            {
                                Vertex v = vertexMemory[vertexIndex];
                                vertices[vertexIndex] = v.vertex;
                                vertexNormals[vertexIndex] = v.normal;
                                uvs[vertexIndex] = v.uv;
                            }
                            else
                            {
                                edgeLoopScale.x = p1.size.x;
                                edgeLoopScale.y = p1.size.y;
                                Vertex v = new Vertex();
                                vertices[vertexIndex] = v.vertex = p1.LocalToWorld(new Vector3(shape.vertices[j].position.x * edgeLoopScale.x, shape.vertices[j].position.y * edgeLoopScale.y, shape.vertices[j].position.z));
                                //vertexNormals[vertexIndex] = v.normal = orientedPoint.LocalToWorldDirection(new Vector3(shape.vertices[j].normal.x * edgeLoopScale.y, shape.vertices[j].normal.y * edgeLoopScale.x, shape.vertices[j].normal.z).normalized);
                                _largestVCoord = _vCoordOffset + (float)(GetLength_0toN(T1) * invUSpan);
                                uvs[vertexIndex] = v.uv = new Vector2(shape.vertices[j].uv.x, _largestVCoord);
                                vertexMemory[vertexIndex] = v;
                            }
                        }


                        // Triangles
                        
                        ti = trianglesAdded * 3;
                        int triangleIndex = trianglesAdded;
                        
                        for (int l = 0; l < shape.lines.Length; l += 3)
                        {
                            // edgeLoop : 0...1
                            // 	     -x   a - c
                            //            | / |
                            //       +x   b - d
                            bool flipTriangles = shape.lines[l + 2] > 0;
                            int a = vertexOffset + shape.lines[l];
                            int b = vertexOffset + shape.lines[l + 1];
                            int c = vertexOffset + shape.lines[l] + verticesInShape;
                            int d = vertexOffset + shape.lines[l + 1] + verticesInShape;
                            triangles[ti++] = a;
                            triangles[ti++] = c;
                            triangles[ti++] = flipTriangles ? d : b;
                            triangleCenters[triangleIndex] = (vertices[a] + vertices[c] + vertices[b]) / 3f;
                            triangleNormals[triangleIndex] = Vector3.Cross((vertices[c] - vertices[a]).normalized, (vertices[b] - vertices[a]).normalized).normalized;
                            trianglesConnectedToVertex[a].Add(triangleIndex);
                            trianglesConnectedToVertex[c].Add(triangleIndex);
                            trianglesConnectedToVertex[b].Add(triangleIndex);
                            vertexMemory[a].normal = vertexNormals[a] = triangleNormals[triangleIndex];
                            vertexMemory[c].normal = vertexNormals[c] = triangleNormals[triangleIndex];
                            vertexMemory[b].normal = vertexNormals[b] = triangleNormals[triangleIndex];
                            triangleIndex++;

                            triangles[ti++] = flipTriangles ? a : c;
                            triangles[ti++] = d;
                            triangles[ti++] = b;
                            triangleCenters[triangleIndex] = (vertices[c] + vertices[d] + vertices[b]) / 3f;
                            triangleNormals[triangleIndex] = Vector3.Cross((vertices[d] - vertices[c]).normalized, (vertices[b] - vertices[c]).normalized).normalized;
                            trianglesConnectedToVertex[c].Add(triangleIndex);
                            trianglesConnectedToVertex[d].Add(triangleIndex);
                            trianglesConnectedToVertex[b].Add(triangleIndex);
                            vertexMemory[c].normal = vertexNormals[c] = triangleNormals[triangleIndex];
                            vertexMemory[d].normal = vertexNormals[d] = triangleNormals[triangleIndex];
                            vertexMemory[b].normal = vertexNormals[b] = triangleNormals[triangleIndex];
                            triangleIndex++;
                        }
                        trianglesAdded += linesInShape * 2;
                    }

                    segmentsBeforeCurve += segmentsInCurve;
                }
               
                verticesAdded += totalSegments * verticesInShape * 2;
                //trianglesAdded += totalSegments * linesInShape * 2;
            }
            else
            {
                for (int c = 0; c < CurveCount; ++c)
                {
                    int segmentsInCurve = GetSegmentsInCurve(c);
                    if (c + 1 > index)
                    { // do not update the point we already updated before ...
                        bezierPoint = GetPoint(c + 1);
                        bezierPoint.CalcSizeAtTime(timeRelativeToSpline);
                        pointStillAnimating = bezierPoint.widthIsAnimating ? Math.Min(pointStillAnimating, c + 1) : pointStillAnimating;
                    }
                    for (int p = (c == 0) ? 0 : 1; p <= segmentsInCurve; ++p)
                    {
                        int offset = (segmentsBeforeCurve + p) * verticesInShape + verticesAdded;
                        //double t = (segmentsBeforeCurve + p) / (double)totalSegments;
                        double T = c + p / (double)(segmentsInCurve);
                        //Debug.Log ("edgeLoop = " + (segmentsBeforeCurve + p) + " | T = " + T);
                        for (int j = 0; j < verticesInShape; j++)
                        {
                            int vertexIndex = offset + j;
                            if (vertexIndex < firstVertexIndexToUpdate)
                            {
                                //Debug.Log ("accessing vertexMemory[" + vertexIndex + "]");
                                Vertex v = vertexMemory[vertexIndex];
                                vertices[vertexIndex] = v.vertex;
                                vertexNormals[vertexIndex] = v.normal;
                                uvs[vertexIndex] = v.uv;
                            }
                            else
                            {
                                orientedPoint = GetInterpolatedOrientedMovingPoint_0toN(T);
                                edgeLoopScale.x = orientedPoint.size.x;
                                edgeLoopScale.y = orientedPoint.size.y;
                                //if (_isBeingDrawn && (c == CurveCount - 1)) {
                                //	float t = (float)(T - (CurveCount-1));
                                //	edgeLoopScale *= 1f - t;
                                //}
                                Vertex v = new Vertex();
                                vertices[vertexIndex] = v.vertex = orientedPoint.LocalToWorld(new Vector3(shape.vertices[j].position.x * edgeLoopScale.x, shape.vertices[j].position.y * edgeLoopScale.y, shape.vertices[j].position.z));
                                vertexNormals[vertexIndex] = v.normal = orientedPoint.LocalToWorldDirection(new Vector3(shape.vertices[j].normal.x * edgeLoopScale.y, shape.vertices[j].normal.y * edgeLoopScale.x, shape.vertices[j].normal.z).normalized);
                                _largestVCoord = _vCoordOffset + (float)(GetLength_0toN(T) * invUSpan);
                                uvs[vertexIndex] = v.uv = new Vector2(shape.vertices[j].uv.x, _largestVCoord);
                                vertexMemory[vertexIndex] = v;
                            }
                        }
                    }
                    segmentsBeforeCurve += segmentsInCurve;
                }
                // TODO: get triangles from a saved buffer too
                ti = trianglesAdded * 3;
                int triangleIndex = trianglesAdded;
                for (int i = 0; i < totalSegments; i++)
                {
                    int offset = i * verticesInShape + verticesAdded;
                    for (int l = 0; l < shape.lines.Length; l += 3)
                    {
                        // edgeLoop : 0...1
                        // 	     -x   a - c
                        //            | / |
                        //       +x   b - d
                        bool flipTriangles = shape.lines[l + 2] > 0;
                        int a = offset + shape.lines[l];
                        int b = offset + shape.lines[l + 1];
                        int c = offset + shape.lines[l] + verticesInShape;
                        int d = offset + shape.lines[l + 1] + verticesInShape;
                        triangles[ti++] = a;
                        triangles[ti++] = c;
                        triangles[ti++] = flipTriangles ? d : b;
                        triangleCenters[triangleIndex] = (vertices[a] + vertices[c] + vertices[b]) / 3f;
                        triangleNormals[triangleIndex] = Vector3.Cross((vertices[c] - vertices[a]).normalized, (vertices[b] - vertices[a]).normalized).normalized;
                        trianglesConnectedToVertex[a].Add(triangleIndex);
                        trianglesConnectedToVertex[c].Add(triangleIndex);
                        trianglesConnectedToVertex[b].Add(triangleIndex);
                        triangleIndex++;

                        triangles[ti++] = flipTriangles ? a : c;
                        triangles[ti++] = d;
                        triangles[ti++] = b;
                        triangleCenters[triangleIndex] = (vertices[c] + vertices[d] + vertices[b]) / 3f;
                        triangleNormals[triangleIndex] = Vector3.Cross((vertices[d] - vertices[c]).normalized, (vertices[b] - vertices[c]).normalized).normalized;
                        trianglesConnectedToVertex[c].Add(triangleIndex);
                        trianglesConnectedToVertex[d].Add(triangleIndex);
                        trianglesConnectedToVertex[b].Add(triangleIndex);
                        triangleIndex++;
                    }
                }
                verticesAdded += edgeLoops * verticesInShape;
                trianglesAdded += totalSegments * linesInShape * 2;
            }

			// end cap after
			/*
			if (_extrudeShapeSettings.drawEndCaps) {
				BezierPoint lastPoint = GetLastPoint ();
				if (lastPoint != null) {
					orientedPoint = (OrientedMovingPoint)lastPoint;
					edgeLoopScale.x = orientedPoint.size.x;
					edgeLoopScale.y = orientedPoint.size.y;
					for (int j = 0; j < verticesInEndCap; ++j) {
						int vertexIndex = verticesAdded + j;
						if (vertexIndex < firstVertexIndexToUpdate) {
							Vertex v = vertexMemory [vertexIndex];
							vertices [vertexIndex] = v.vertex;
							vertexNormals [vertexIndex] = v.normal;
							uvs [vertexIndex] = v.uv;
						} else {
							Vertex v = new Vertex ();
							vertices [vertexIndex] = v.vertex = orientedPoint.LocalToWorld (new Vector3(shape.endCapVertices [j].position.x * edgeLoopScale.x, shape.endCapVertices [j].position.y * edgeLoopScale.y, shape.endCapVertices [j].position.z));
							vertexNormals [vertexIndex] = v.normal = orientedPoint.LocalToWorldDirection (shape.endCapVertices [j].normal);
							uvs [vertexIndex] = v.uv = shape.endCapVertices [j].uv;
							vertexMemory [vertexIndex] = v;
						}
					}
					for (int l = 0; l < trianglesInEndCap; ++l) {
						ti = (trianglesAdded + l) * 3;
						triangles [ti] = verticesAdded + shape.endCapTriangels [l*3 + 0];
						triangles [ti + 1] = verticesAdded + shape.endCapTriangels [l*3 + 1];
						triangles [ti + 2] = verticesAdded + shape.endCapTriangels [l*3 + 2];
					}
					verticesAdded += verticesInEndCap;
					trianglesAdded += trianglesInEndCap;
				}
			}
			*/
			vertexMemorySize = vertexCount;
			triangleMemorySize = triangleCount;


            if (!doubleVertices)
            {
                // calculate vertex normals from face normals
                int t_count;
                for (int v = 0; v < vertexCount; ++v)
                {
                    vertexNormals[v] = Vector3.zero;
                    t_count = trianglesConnectedToVertex[v].Count;
                    for (int t = 0; t < t_count; ++t)
                    {
                        vertexNormals[v] += triangleNormals[trianglesConnectedToVertex[v][t]];
                    }
                    vertexNormals[v] /= (float)t_count;
                    vertexNormals[v].Normalize();
                    if (vertexMemorySize < v)
                        vertexMemory[v].normal = vertexNormals[v];
                }
            }
		}



		if (_updateFromInspector) {
			MeshFilter anyMeshFilter = gameObject.GetComponent<MeshFilter> ();
			if (anyMeshFilter == null) {
				anyMeshFilter = gameObject.AddComponent<MeshFilter> ();
			}
			Mesh anyMesh = anyMeshFilter.sharedMesh;
			if (anyMesh == null) {
				anyMeshFilter.sharedMesh = anyMesh = new Mesh ();
				anyMesh.name = "Mesh [inspector]";
			}
			anyMesh.Clear ();
			if (shape != null) {
				anyMesh.vertices = vertices;
				anyMesh.normals = vertexNormals;
				anyMesh.uv = uvs;
				anyMesh.triangles = triangles;
			}
		} else {
			mesh.Clear ();
			if (shape != null) {
				mesh.vertices = vertices;
				mesh.normals = vertexNormals;
				mesh.uv = uvs;
				mesh.triangles = triangles;
				//Debug.Log ("BezierSpline.UpdateMeshFromPoint(" + index + ") : vertices = " + vertices.Length);
			}
		}

		if (pointStillAnimating < PointCount && splineWidthSettings.keepAnimatingAfterSplineEnded) {
			_dirtyMeshFromPoint = Math.Max(pointStillAnimating - 1, 0);
		} else {
			_dirtyMeshFromPoint = -1;
		}
	}



	// UNITY INTERFACE
	public void Reset() {
		points.Clear();
	}
		
	public void Start() {
		
		ForceNewMesh ();

		/*
		// array test
		float[] array0 = new float[] {0,1,2,3,4,5,6,7,8,9,10};
		float[] array1 = array0;
		array0 = new float[] { 5, 6, 7, 8,9,10 };
		array1 [5] = 50;
		Debug.Log ("Array0 test: " + array0.Sample (0.51f));
		Debug.Log ("Array1 test: " + array1.Sample (0.51f));
		*/

	}

	public void Update() {

        //Debug.Log("Spline " + id + " is dirty from point " + _dirtySplineFromPoint + ". Mesh from " + _dirtyMeshFromPoint);

		if (_dirtySplineFromPoint >= 0) {
			UpdateSplineFromPoint (_dirtySplineFromPoint);
		}

		if (_dirtyLengthTableFromPoint >= 0) {
			UpdateLengthTableFromPoint (_dirtyLengthTableFromPoint);
		}

		if (_dirtySegmentsFromPoint >= 0) {
			UpdateSegmentsFromPoint (_dirtySegmentsFromPoint);
		}

		if (_dirtyMeshFromPoint >= 0) {
			UpdateMeshFromPoint (_dirtyMeshFromPoint, IsBeingDrawnByWand || splineWidthSettings.keepAnimatingAfterSplineEnded);
		}

        if (splinePathSettings.simplifyStraightSectionsBelowAngle > 0f && splinePathSettings.simplifyStraightSectionsBelowWidthDiff > 0f)
        {
            if (!_doneSimplifyingStraightSections && (IsBeingDrawnByWand || splinePathSettings.keepSimplifyingAfterSplineEnds))
            {
                SimplifyStraightSections(1);
            }
        }
	}
	public void UpdateFromInspector() {
		_updateFromInspector = true;
		Update ();
	}

}
