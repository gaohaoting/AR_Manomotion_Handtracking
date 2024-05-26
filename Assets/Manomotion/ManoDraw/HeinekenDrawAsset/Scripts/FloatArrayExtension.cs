using UnityEngine;

public static class FloatArrayExtensions {
	public static float Sample( this float[] fArr, float t, int size = -1){
		if (size < 0) {
			size = fArr.Length;
		}
		if(size == 0){
			//Debug.LogError("Unable to sample array - it has no elements" );
			return t;
		}
		if(size == 1)
			return fArr[0];
		float iFloat = t * (size-1);
		int idLower = Mathf.FloorToInt(iFloat);
		int idUpper = Mathf.FloorToInt(iFloat + 1);
		if( idUpper >= size )
			return fArr[size-1];
		if( idLower < 0 )
			return fArr[0];
		return Mathf.Lerp( fArr[idLower], fArr[idUpper], iFloat - idLower);
	}
}