using UnityEngine;
using System.Collections;

public class Interpolation {

	// LINEAR
	public static float LinearInterpolation01(float pct) {
		return pct;
	}


	// COSINE
	public static float CosineInterpolation01(float pct) {
		return (1f - Mathf.Cos(pct * Mathf.PI)) * 0.5f;
	}
	public static float CosineInterpolation(float pct, float a, float b) {
		float mu = CosineInterpolation01(pct);
		return (1f - mu) * a + mu * b;
	}


	// EXPONENTIAL
	public static float InverseExponentialInterpolation01(float pct) {
		return 1f - (1f-pct)*(1f-pct);
	}


	// CUBIC
	public static float CubicInterpolation01(float pct) {
		float oneMinusPct = 1f - pct;
		return 
			3f * oneMinusPct * pct * pct +
			pct * pct * pct;
	}
	public static float CubicInterpolation(float pct, float a, float b) {
		float oneMinusPct = 1f - pct;
		return 
			oneMinusPct * oneMinusPct * oneMinusPct * a +
			3f * oneMinusPct * oneMinusPct * pct * a +
			3f * oneMinusPct * pct * pct * b +
			pct * pct * pct * b;
	}
	public static float CubicInterpolation(float pct, float a, float b, float c, float d) {
		float oneMinusPct = 1f - pct;
		return 
			oneMinusPct * oneMinusPct * oneMinusPct * a +
			3f * oneMinusPct * oneMinusPct * pct * b +
			3f * oneMinusPct * pct * pct * c +
			pct * pct * pct * d;
	}
}
