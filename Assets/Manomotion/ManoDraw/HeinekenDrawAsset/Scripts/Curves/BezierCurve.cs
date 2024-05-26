using UnityEngine;
using System.Collections;

public class BezierCurve
{
    // DO NOT STORE ANY MEMBER DATA IN CURVE!
    // IT IS A STRUCTURE NEVER HELD CONSTANTLY!

    // CONSTRUCTORS
    public BezierPoint p0, p1;
    public BezierCurve() { }
    public BezierCurve(BezierPoint p0, BezierPoint p1)
    {
        this.p0 = p0;
        this.p1 = p1;
    }

    // PRIVATE
    private Vector3 _p0 { get { return p0.position; } }
    private Vector3 _p1 { get { return p0.cp1; } }
    private Vector3 _p2 { get { return p1.cp0; } }
    private Vector3 _p3 { get { return p1.position; } }

    //private float length = -1f;
    //private float[] lengthTable;

    private Vector3 GetPoint(float t)
    {
        //t = Mathf.Clamp01 (t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * oneMinusT * _p0 +
            3f * oneMinusT * oneMinusT * t * _p1 +
            3f * oneMinusT * t * t * _p2 +
            t * t * t * _p3;
    }

    private Vector3 GetFirstDerivative(float t)
    {
        //t = Mathf.Clamp01 (t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (_p1 - _p0) +
            6f * oneMinusT * t * (_p2 - _p1) +
            3f * t * t * (_p3 - _p2);
    }

    private Vector3 GetTangent(float t)
    {
        return GetFirstDerivative(t).normalized;
    }

    private Vector3 GetNormal(float t, Vector3 up)
    {
        Vector3 tng = GetTangent(t);
        Vector3 binormal = Vector3.Cross(up, tng).normalized;
        return Vector3.Cross(tng, binormal);
    }

    // PUBLIC
    public OrientedMovingPoint GetInterpolatedOrientedMovingPointUnclamped(double t)
    {
        float tF = (float)t;
        Vector3 point = GetPoint(tF);
        Vector3 tangent = GetTangent(tF);
        Quaternion rotation = Quaternion.identity;
        if (tangent.sqrMagnitude == 0)
        {
            // HACK: to avoid identity rotation
            Vector3 curveLine = _p3 - _p0;
            if (curveLine.sqrMagnitude == 0)
            {
                Debug.LogWarning("Need correct rotation when p0 == p1");
                //rotation = p0.initialRotation;
            }
            else if (t == 0f && _p0 == _p1)
            {
                rotation.SetLookRotation(_p3 - _p0);
            }
            else if (t == 1f && _p2 == _p3)
            {
                rotation.SetLookRotation(_p3 - _p0);
            }
            // SOLVED: Why does this happen? Because p0.pos == p0.cp1?
            //Debug.Log("Tangent is zero, t = " + t);
        }
        else
        {
            //rotation = Quaternion.SlerpUnclamped(p0.rotation, p1.rotation, tF);
            Vector3 interpolatedUp = Vector3.SlerpUnclamped(p0.rotation * Vector3.up, p1.rotation * Vector3.up, tF);
            rotation.SetLookRotation(tangent, interpolatedUp);
        }
        Vector3 interpolatedVelocity = Vector3.SlerpUnclamped(p0.velocity, p1.velocity, tF);
        //float interpolatedSpeed = Mathf.Lerp (p0.speed, p1.speed, tF);
        //float interpolatedRadius = Interpolation.LinearInterpolation(tF, p0.radius, p1.radius);
        //float interpolatedRadius = Interpolation.CosineInterpolation(tF, p0.radius, p1.radius);


        // TODO : Other interpolation?
        Vector3 interpolatedSize = Vector3.zero;
        interpolatedSize.x = Interpolation.CubicInterpolation(tF, p0.size.x, p1.size.x);
        interpolatedSize.y = Interpolation.CubicInterpolation(tF, p0.size.y, p1.size.y);
        return new OrientedMovingPoint(point, rotation, interpolatedVelocity, interpolatedSize);
    }


    public double CalcLengthTable(double[] table)
    {
        double totalLength = 0f;
        table[0] = totalLength;
        Vector3 prev = _p0;
        for (int i = 1; i < table.Length; i++)
        {
            double t = ((double)i) / (table.Length - 1);
            Vector3 pt = GetPoint((float)t);
            double diff = (prev - pt).magnitude;
            totalLength += diff;
            table[i] = totalLength;
            prev = pt;
        }
        return totalLength;
    }

    /*public float GetLength(float t) {
		if (length < 0f) {
			lengthTable = new float[subSegments+1]; // TODO: set from num segments
			//Debug.Log ("Calculate new length table for curve.");
			length = CalcLengthTable (lengthTable);
		}
		return lengthTable.Sample (t);
	}*/

}
