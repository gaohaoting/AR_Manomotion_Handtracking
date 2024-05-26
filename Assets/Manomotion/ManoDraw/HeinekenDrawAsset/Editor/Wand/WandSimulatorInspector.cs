using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WandSimulator))]
public class WandSimulatorInspector : WandBaseInspector {

	private WandSimulator wand;

	private void OnSceneGUI() {
		wand = target as WandSimulator;
	}

	public override void OnInspectorGUI() {
		
		base.OnInspectorGUI ();

		wand = target as WandSimulator;

		GUILayout.Space (20);
		GUILayout.Label ("Wand Simulator Inspector");

		if (GUILayout.Button (wand.IsDrawButtonDown() ? "Release Draw Button" : "Press Draw Button")) {
			wand.OnDrawButton (!wand.IsDrawButtonDown ());
			EditorUtility.SetDirty (wand);
		}

		GUILayout.Space (20);
		GUILayout.Label ("Wand Default Inspector");
		DrawDefaultInspector ();
	}
}

