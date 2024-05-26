using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor {

	private BezierSpline spline;
	private Transform handleTransform;
	private Quaternion handleRotation;

	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;
	private int selectedIndex = -1;
	private int selectedControlPoint = -1;

	private bool drawTriangleNormals = false;
	private bool drawVertexNormals = false;
	private bool drawCurveTangents = true;
	private bool drawCurveUps = true;
    private bool drawInitialForwards = false;
    private bool drawInitialUps = false;

    private void OnSceneGUI() {
		spline = target as BezierSpline;
		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		// draw points and control points
		BezierPoint p0 = ShowPoint(0);
		for (int p = 1; p < spline.PointCount; p++) {
			BezierPoint p1 = ShowPoint (p);

			Handles.color = Color.gray;
			Handles.DrawLine (p0.position, p0.cp1);
			Handles.DrawLine (p1.cp0, p1.position);

			p0 = p1;
		}

		// draw sub-segments
		Vector3 prevTransformedPointPosition = handleTransform.TransformPoint (spline.GetInterpolatedOrientedMovingPoint_0toN (0.0).position);
		OrientedMovingPoint point, transformedPoint;
		for (int c = 0; c < spline.CurveCount; ++c) {
			int segments = spline.GetSegmentsInCurve (c);
			for (int s = 1; s <= segments; ++s) {
				float t = (float)c + s / (float)segments;
				point = spline.GetInterpolatedOrientedMovingPoint_0toN (t);
				transformedPoint = new OrientedMovingPoint(
					handleTransform.TransformPoint (point.position),
					handleRotation * point.rotation,
					handleRotation * point.velocity,
					point.size
				);


				Handles.color = Color.white;
				Handles.DrawLine (prevTransformedPointPosition, transformedPoint.position);

				// forward
				if (drawCurveTangents) {
					Handles.color = Color.green;
					Handles.DrawLine (transformedPoint.position, transformedPoint.LocalToWorld (Vector3.forward * 0.05f));
				}
				// up
				if (drawCurveUps) {
					Handles.color = Color.red;
					Handles.DrawLine (transformedPoint.position, transformedPoint.position + handleRotation * transformedPoint.LocalToWorldDirection (Vector3.up * 0.05f));
				}

				// Debug text
				/*int index = (i - 1) / BezierCurve.SEGMENTS;
				if (selectedIndex == index || selectedIndex - 1 == index) {
					string text = "t = " + t + " | length = " + spline.GetLength (t) + " | segments before = " + spline.GetSegmentsBeforePoint(;
					Handles.Label(transformedPoint.position, text);
				}*/

				prevTransformedPointPosition = transformedPoint.position;
			}
		}

		if (drawVertexNormals) {
			for (int i = 0; i < spline.VertexCount; ++i) {
				Vector3 vertexPosition = spline.GetVertexPosition (i);
				Vector3 vertexNormal = spline.GetVertexNormal (i);
				Handles.color = Color.yellow;
				Handles.DrawLine (handleTransform.TransformPoint (vertexPosition), handleTransform.TransformPoint (vertexPosition + vertexNormal * 0.005f));
			}
		}
		if (drawTriangleNormals) {
			for (int i = 0; i < spline.TriangleCount; ++i) {
				Vector3 trianglePosition = spline.GetTriangleCenterPosition (i);
				Vector3 triangleNormal = spline.GetTriangleNormal (i);
				Handles.color = Color.blue;
				Handles.DrawLine (handleTransform.TransformPoint (trianglePosition), handleTransform.TransformPoint (trianglePosition + triangleNormal * 0.005f));
			}
		}

		if (selectedIndex >= spline.PointCount)
			selectedIndex = -1;
	}

	private BezierPoint ShowPoint (int index) {
		BezierPoint point = spline.GetPoint (index);
		if (point == null) {
			return null;
		}
		BezierPoint transformedPoint = new BezierPoint ();
		transformedPoint.position = handleTransform.TransformPoint (point.position);
		transformedPoint.cp0 = handleTransform.TransformPoint (point.cp0);
		transformedPoint.cp1 = handleTransform.TransformPoint (point.cp1);
		transformedPoint.rotation = handleRotation * point.rotation;
        transformedPoint.initialRotation = point.initialRotation;

        //Quaternion pointHandleRotation = (spline.splineSettings.useInitialRotationDirectly) ? transformedPoint.rotation : handleRotation;
        Quaternion pointHandleRotation = handleRotation;

        // POINT
        Handles.color = Color.white;
		float size = HandleUtility.GetHandleSize (transformedPoint.position);
		if (Handles.Button (transformedPoint.position, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) 
		{
			selectedIndex = index;
			selectedControlPoint = -1;
			Repaint ();
		}
		if (selectedIndex == index && selectedControlPoint < 0) {
			if (spline.IsExistingPointsEditable) {
				EditorGUI.BeginChangeCheck ();
				transformedPoint.position = Handles.DoPositionHandle (transformedPoint.position, pointHandleRotation);
				if (EditorGUI.EndChangeCheck ()) {
					point.position = handleTransform.InverseTransformPoint (transformedPoint.position);
					spline.SetPointLocal(index, point);
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
				}
			} else {
				Handles.DoPositionHandle (transformedPoint.position, transformedPoint.rotation);
			}
		}

		// CONTROL POINT 0
		Handles.color = Color.gray;
		size = HandleUtility.GetHandleSize (transformedPoint.cp0);
		if (Handles.Button (transformedPoint.cp0, handleRotation, size * handleSize * 0.8f, size * pickSize, Handles.DotHandleCap)) {
			selectedIndex = index;
			selectedControlPoint = 0;
			Repaint ();
		}
		if (selectedIndex == index && selectedControlPoint == 0) {
			if (spline.IsExistingPointsEditable) {
				EditorGUI.BeginChangeCheck ();
				transformedPoint.cp0 = Handles.DoPositionHandle (transformedPoint.cp0, pointHandleRotation);
				if (EditorGUI.EndChangeCheck ()) {
					point.cp0 = handleTransform.InverseTransformPoint (transformedPoint.cp0);
					spline.SetPointLocal(index, point);
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
				}
			} else {
				Handles.DoPositionHandle (transformedPoint.cp0, transformedPoint.rotation);
			}
		}

		// CONTROL POINT 1
		Handles.color = Color.gray;
		size = HandleUtility.GetHandleSize (transformedPoint.cp1);
		if (Handles.Button (transformedPoint.cp1, handleRotation, size * handleSize * 0.8f, size * pickSize, Handles.DotHandleCap)) {
			selectedIndex = index;
			selectedControlPoint = 1;
			Repaint ();
		}
		if (selectedIndex == index && selectedControlPoint == 1) {
			if (spline.IsExistingPointsEditable) {
				EditorGUI.BeginChangeCheck ();
				transformedPoint.cp1 = Handles.DoPositionHandle (transformedPoint.cp1, pointHandleRotation);
				if (EditorGUI.EndChangeCheck ()) {
					point.cp1 = handleTransform.InverseTransformPoint (transformedPoint.cp1);
					spline.SetPointLocal (index, point);
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
				}
			} else {
				Handles.DoPositionHandle (transformedPoint.cp1, transformedPoint.rotation);
			}
		}

        // Debug text
		if (selectedIndex == index) {
			float t = index / (float)(spline.PointCount - 1);
            //string text = "t = " + t + " | length = " + spline.GetLength (t);

            string text = "segments before point = " + spline.GetSegmentsBeforePoint(index) + "\n" + point.GetInfoText();
            Handles.Label(transformedPoint.position + new Vector3(0,-0.03f,0), text);
		}

        if (drawInitialForwards)
        {
            Handles.color = Color.magenta;
            Handles.DrawLine(transformedPoint.position, transformedPoint.position + transformedPoint.initialRotation * Vector3.forward * 0.05f);
        }
        if (drawInitialUps)
        {
            Handles.color = Color.cyan;
            Handles.DrawLine(transformedPoint.position, transformedPoint.position + transformedPoint.initialRotation * Vector3.up * 0.05f);
        }

        return transformedPoint;
	}

	public override void OnInspectorGUI() {
		//DrawDefaultInspector ();
		spline = target as BezierSpline;

		GUILayout.Label ("ID : " + spline.id);
		GUILayout.Label ("Points : " + spline.PointCount);
		GUILayout.Label ("Total Length : " + spline.GetTotalLength());
		GUILayout.Label ("Total Segments : " + spline.GetSegmentsBeforePoint(spline.PointCount-1));
		GUILayout.Label ("Vertex Count : " + spline.VertexCount);
		GUILayout.Label ("Triangle Count : " + spline.TriangleCount);
        GUILayout.Label("Was split at front : " + (spline.wasSplitAtFront ? "Yes" : "No"));
        GUILayout.Label("Was split at end : " + (spline.wasSplitAtEnd ? "Yes" : "No"));

        EditorGUI.BeginChangeCheck ();
		drawVertexNormals = GUILayout.Toggle (drawVertexNormals, "Draw Vertex Normals");
		if (EditorGUI.EndChangeCheck ()) {
			EditorUtility.SetDirty (spline);
		}
		EditorGUI.BeginChangeCheck ();
		drawTriangleNormals = GUILayout.Toggle (drawTriangleNormals, "Draw Triangle Normals");
		if (EditorGUI.EndChangeCheck ()) {
			EditorUtility.SetDirty (spline);
		}
		EditorGUI.BeginChangeCheck ();
		drawCurveTangents = GUILayout.Toggle (drawCurveTangents, "Draw Curve Tangents");
		if (EditorGUI.EndChangeCheck ()) {
			EditorUtility.SetDirty (spline);
		}
		EditorGUI.BeginChangeCheck ();
		drawCurveUps = GUILayout.Toggle (drawCurveUps, "Draw Curve Ups");
		if (EditorGUI.EndChangeCheck ()) {
			EditorUtility.SetDirty (spline);
        }
        EditorGUI.BeginChangeCheck();
        drawInitialUps = GUILayout.Toggle(drawInitialUps, "Draw Initial Ups");
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(spline);
        }
        EditorGUI.BeginChangeCheck();
        drawInitialForwards = GUILayout.Toggle(drawInitialForwards, "Draw Initial Forwards");
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(spline);
        }


        GUILayout.Space (20);
		GUILayout.Label ("Debug options (change settings instead)");

		EditorGUI.BeginChangeCheck ();
		int brushIndex = EditorGUILayout.IntField ("Brush Index", spline.brushIndex);
		if (EditorGUI.EndChangeCheck ()) {
			spline.SetBrushIndex (brushIndex);
            spline.SetDirtySplineFromPoint(0);
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}
		if (GUILayout.Button ("Apply Brush")) {
			spline.SetBrushIndex (spline.brushIndex);
            spline.UpdateFromInspector();
        }
		EditorGUI.BeginChangeCheck ();
		Color32 color = EditorGUILayout.ColorField ("Color",  spline.color);
		if (EditorGUI.EndChangeCheck ()) {
			spline.color = color;
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}

		EditorGUI.BeginChangeCheck ();
		bool isEditable = EditorGUILayout.Toggle ("Editable", spline.IsExistingPointsEditable);
		if (EditorGUI.EndChangeCheck ()) {
			spline.IsExistingPointsEditable = isEditable;
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}
		EditorGUILayout.Toggle ("Is Being Drawn", spline.IsBeingDrawnByWand);


		
		/*EditorGUI.BeginChangeCheck ();
		float widthAnimationTime = EditorGUILayout.FloatField ("Width Animation Time", spline.widthSettings.widthAnimationTime);
		if (EditorGUI.EndChangeCheck ()) {
			spline.widthSettings.widthAnimationTime = widthAnimationTime;
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}*/


		if (GUILayout.Button ("Add Straight Point")) {
			BezierPoint bp = spline.GetLastPoint ();
			if (bp != null) {
				bp = spline.AddPointLocal (new OrientedMovingPoint(
					bp.position + bp.rotation * Vector3.forward, 
					bp.rotation, 
					bp.rotation * Vector3.forward,
					bp.size
				));
			} else {
				bp = spline.AddPointLocal( new OrientedMovingPoint(Vector3.zero, Quaternion.identity, Vector3.forward, new Vector3(1f,1f,0f)));
			}
			bp.createTimeRelativeToSpline = 0;
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}
		if (GUILayout.Button ("Add 100 Curved Points")) {
			int i = 0;
			Vector3 wandRotation = Vector3.zero;
			Vector3 noise;
			BezierPoint bp = spline.GetLastPoint ();
			if (bp == null) {
				bp = spline.AddPointLocal( new OrientedMovingPoint(Vector3.zero, Quaternion.identity, Vector3.forward, new Vector3(1f,1f,0f)));
				bp.createTimeRelativeToSpline = 0;
				++i;
			}
			int style = 1;
			switch (style) {
			case 0:
				for (; i < 100; ++i) {
					float scale = 0.05f;
					noise = new Vector3 (Random.value, Random.value, Random.value) * 0.05f;
					wandRotation.x += (Random.value - 0.5f) * 1f;
					Vector3 offsetGlobal = scale * (bp.rotation * Vector3.forward * (1f + noise.x) + bp.rotation * Vector3.up * (0.1f + noise.y) + bp.rotation * Vector3.right * (0.01f + noise.z));
					bp = spline.AddPointLocal (new OrientedMovingPoint (
						bp.position + offsetGlobal, 
						Quaternion.Euler (wandRotation), 
						offsetGlobal,
						bp.size
					));
					bp.createTimeRelativeToSpline = 0;
					spline.UpdateFromInspector ();
				}
				break;
			case 1:
				float radius = 0.5f;
				Vector3 center = bp.position - new Vector3 (0, radius, 0);
				float t = 0;
				for (; i < 100; ++i) {
					t = i / 10f;
					noise = new Vector3 (Random.value, Random.value, Random.value) * 0.5f;
					//wandRotation.y += (Random.value - 0.5f) * 1f;
					wandRotation.z = 45f; 
					//Vector3 offsetGlobal = (bp.rotation * Vector3.forward * (1f + noise.x) + bp.rotation * Vector3.up * (0.1f + noise.y) + bp.rotation * Vector3.right * (0.01f + noise.z));
					bp = spline.AddPointLocal (new OrientedMovingPoint (
						center + new Vector3(i * 0.001f, radius * Mathf.Cos(t), radius * Mathf.Sin(t)),
						Quaternion.Euler (wandRotation), 
						Vector3.zero,
						bp.size
					));
					bp.createTimeRelativeToSpline = 0;
					spline.UpdateFromInspector ();
				}
				break;
			}

			spline.SetDirtySplineFromPoint (0);
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}
		if (spline.IsExistingPointsEditable && selectedIndex > -1 && GUILayout.Button ("Remove Selected Point")) {
			spline.RemovePoint (selectedIndex);
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}
		if (GUILayout.Button ("Remove Last Point")) {
			spline.RemovePoint (spline.PointCount-1);
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}
		if (GUILayout.Button ("Remove All Points")) {
			while (spline.PointCount > 0) {
				spline.RemovePoint (spline.PointCount - 1);
			}
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
        }
        if (selectedIndex >= 0 && selectedIndex < spline.PointCount && GUILayout.Button("Remove Selected Point"))
        {
            spline.RemovePoint(selectedIndex);
            spline.UpdateFromInspector();
            EditorUtility.SetDirty(spline);
        }


        if (GUILayout.Button ("Update Control Points")) {
			spline.SetDirtySplineFromPoint (0);
			spline.UpdateFromInspector ();
			EditorUtility.SetDirty (spline);
		}


		if (GUILayout.Button ("Recalculate Mesh Normals")) {
			spline.RecalculateMeshNormalsUnity ();
			EditorUtility.SetDirty (spline);
		}


		// draw selected point
		if (selectedIndex >= 0 && selectedIndex < spline.PointCount) {
			DrawSelectedPointInspector ();
		}
	}

	private void DrawSelectedPointInspector() {
		GUILayout.Space (20);
		GUILayout.Label ("Selected Point");
		GUILayout.Label ("Index : " + selectedIndex);
		BezierPoint point = spline.GetPoint (selectedIndex);
		if (point != null) {
			if (spline.IsExistingPointsEditable) {
				EditorGUI.BeginChangeCheck ();
				Vector3 position = EditorGUILayout.Vector3Field ("Position", point.position);
				if (EditorGUI.EndChangeCheck ()) {
					spline.SetPointPositionLocal (selectedIndex, position);
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
				}
				EditorGUI.BeginChangeCheck ();
				Vector3 rotation = EditorGUILayout.Vector3Field ("Rotation", point.rotation.eulerAngles);
				if (EditorGUI.EndChangeCheck ()) {
					spline.SetPointRotationLocal (selectedIndex, Quaternion.Euler (rotation));
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
				}
				EditorGUI.BeginChangeCheck ();
				Vector3 initialRotation = EditorGUILayout.Vector3Field ("Initial Rotation", point.initialRotation.eulerAngles);
				if (EditorGUI.EndChangeCheck ()) {
					spline.SetPointInitialRotationLocal (selectedIndex, Quaternion.Euler (initialRotation));
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
				}
				EditorGUI.BeginChangeCheck ();
				Vector3 velocity = EditorGUILayout.Vector3Field ("Velocity", point.velocity);
				if (EditorGUI.EndChangeCheck ()) {
					spline.SetPointVelocityLocal(selectedIndex, velocity);
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
				}
				EditorGUI.BeginChangeCheck ();
				float widthTarget = EditorGUILayout.FloatField ("Width Target", point.widthTarget);
				if (EditorGUI.EndChangeCheck ()) {
					point.widthTarget = widthTarget;
					spline.SetDirtyMeshFromPoint (selectedIndex);
					spline.UpdateFromInspector ();
					EditorUtility.SetDirty (spline);
                }
                EditorGUI.BeginChangeCheck();
                float thicknessTarget = EditorGUILayout.FloatField("Thickness Target", point.thicknessTarget);
                if (EditorGUI.EndChangeCheck())
                {
                    point.thicknessTarget = thicknessTarget;
                    spline.SetDirtyMeshFromPoint(selectedIndex);
                    spline.UpdateFromInspector();
                    EditorUtility.SetDirty(spline);
				}
				EditorGUI.BeginChangeCheck();
				float widthPostClampScaleFactor = EditorGUILayout.FloatField("Width Post-Clamp Scale Factor", point.initialWidthPostClampScaleFactor);
				if (EditorGUI.EndChangeCheck())
				{
					point.initialWidthPostClampScaleFactor = widthPostClampScaleFactor;
					spline.SetDirtyMeshFromPoint(selectedIndex);
					spline.UpdateFromInspector();
					EditorUtility.SetDirty(spline);
				}
				EditorGUI.BeginChangeCheck();
				float customParameter = EditorGUILayout.FloatField("Custom Parameter", point.initialCustomParameter);
				if (EditorGUI.EndChangeCheck())
				{
					point.initialCustomParameter = customParameter;
					spline.SetDirtyMeshFromPoint(selectedIndex);
					spline.UpdateFromInspector();
					EditorUtility.SetDirty(spline);
				}
            } else {
				EditorGUILayout.Vector3Field ("Position", point.position);
				EditorGUILayout.Vector3Field ("Rotation", point.rotation.eulerAngles);
				EditorGUILayout.Vector3Field ("Velocity", point.velocity);
				EditorGUILayout.FloatField ("Width Target", point.widthTarget);
				EditorGUILayout.FloatField ("Thickness Target", point.thicknessTarget);
				EditorGUILayout.FloatField("Width Post-Clamp Scale Factor", point.initialWidthPostClampScaleFactor);
				EditorGUILayout.FloatField("Custom Parameter", point.initialCustomParameter);
			}
			EditorGUILayout.FloatField ("Speed", point.speed);
			EditorGUILayout.FloatField ("Width", point.size.x);
			EditorGUILayout.FloatField ("Thickness", point.size.y);
			EditorGUILayout.FloatField ("Time", point.createTimeRelativeToSpline);
		}
	}
}
