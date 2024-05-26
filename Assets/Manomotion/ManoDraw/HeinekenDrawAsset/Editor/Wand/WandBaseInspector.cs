using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WandBase))]
public class WandBaseInspector : Editor {

	private WandBase wandBase;

	private void OnSceneGUI() {
		wandBase = target as WandBase;
	}

	public override void OnInspectorGUI() {

		wandBase = target as WandBase;

		//GUILayout.Space (20);
		GUILayout.Label ("Wand Base Inspector");

		Settings settings = Settings.GetInstance ();
		string[] brushNames = settings.GetListOfBrushNames ();

		EditorGUI.BeginChangeCheck ();
		int selectedBrush = EditorGUILayout.Popup ("Brush", wandBase.selectedBrush, brushNames);
		if (EditorGUI.EndChangeCheck ()) {
			wandBase.selectedBrush = selectedBrush;
			EditorUtility.SetDirty (wandBase);
		}

		EditorGUI.BeginChangeCheck ();
		BezierSplineSceneManager sceneManager = (BezierSplineSceneManager)EditorGUILayout.ObjectField ("Scene Manager", wandBase.sceneManager, typeof(BezierSplineSceneManager), true);
		if (EditorGUI.EndChangeCheck ()) {
			wandBase.sceneManager = sceneManager;
			EditorUtility.SetDirty (wandBase);
		}

		/*EditorGUI.BeginChangeCheck ();
		BezierUDPSender udpSender = (BezierUDPSender)EditorGUILayout.ObjectField ("UDP Sender", wandBase.udpSender, typeof(BezierUDPSender), true);
		if (EditorGUI.EndChangeCheck ()) {
			wandBase.udpSender = udpSender;
			EditorUtility.SetDirty (wandBase);
        }*/

		GUILayout.Label("Min Sample Distance : " + wandBase.minSampleDistance);

        if (GUILayout.Button("Undo"))
        {
			wandBase.sceneManager.Undo();
			EditorUtility.SetDirty(wandBase);
        }
        if (GUILayout.Button("Undo All"))
        {
			wandBase.sceneManager.UndoAll();
			EditorUtility.SetDirty(wandBase);
        }
        if (GUILayout.Button("Redo"))
        {
			wandBase.sceneManager.Redo();
			EditorUtility.SetDirty(wandBase);
        }

        //GUILayout.Space (20);
        //GUILayout.Label ("Wand Default Inspector");
        //DrawDefaultInspector ();
    }
}

