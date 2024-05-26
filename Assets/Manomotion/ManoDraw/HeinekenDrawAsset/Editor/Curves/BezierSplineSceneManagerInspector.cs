using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSplineSceneManager))]
public class BezierSplineSceneManagerInspector : Editor {

	private BezierSplineSceneManager sceneManager;


	public override void OnInspectorGUI() {

		DrawDefaultInspector ();

		sceneManager = target as BezierSplineSceneManager;

		EditorGUILayout.ObjectField("Current Spline", sceneManager.GetCurrentSpline(), typeof(BezierSpline), true);
		EditorGUILayout.ObjectField("Last Ended Spline", sceneManager.GetLastEndedSpline(), typeof(BezierSpline), true);
		EditorGUILayout.LabelField ("On spline created delegates : " + sceneManager.OnSplineCreatedDelegateCount);
		EditorGUILayout.LabelField ("On spline ended delegates : " + sceneManager.OnSplineEndedDelegateCount);
        EditorGUILayout.LabelField("Time Scale Factor : " + sceneManager.timeScaleFactor);

        GUILayout.Space(20);
        if (GUILayout.Button("Undo All"))
        {
            sceneManager.UndoAll();
        }

        Repaint();
	}
}
