using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierPackageReceiverOSC))]
public class BezierPackageReceiverOSCInspector : Editor {

	private BezierPackageReceiverOSC receiver;

	private void OnSceneGUI() {
		//receiver = target as UDPReceive;
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		receiver = target as BezierPackageReceiverOSC;

		if (GUILayout.Button ("Clear Default OSC Mappings")) {
			receiver.ClearAllDefaultOscMappings ();
			EditorUtility.SetDirty (receiver);
		}
		if (GUILayout.Button ("Add Default OSC Mappings")) {
			receiver.AddDefaultOscMappings ();
			EditorUtility.SetDirty (receiver);
		}
	}
}