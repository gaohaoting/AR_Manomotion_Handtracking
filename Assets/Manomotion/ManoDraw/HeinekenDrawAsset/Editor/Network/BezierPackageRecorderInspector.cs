using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierPackageRecorder))]
public class BezierPackageRecorderInspector : Editor {

	private BezierPackageRecorder recorder;

	private void OnSceneGUI() {
		//sender = target as UDPSend;
	}

	public override void OnInspectorGUI() {

        DrawDefaultInspector ();

		recorder = target as BezierPackageRecorder;

		GUILayout.Space (20);
		GUILayout.Label ("Status : " + (recorder.IsRecording() ? "Recording" : "Off"));

		if (GUILayout.Button ((recorder.IsRecording() ? "New File" : "Record"))) {
            recorder.StartRecording();
			EditorUtility.SetDirty (recorder);
		}
		if (GUILayout.Button ("Stop")) {
            recorder.StopRecording();
			EditorUtility.SetDirty (recorder);
		}

		// Forces repainting the inspector while it is selected
		//Repaint ();
	}
}