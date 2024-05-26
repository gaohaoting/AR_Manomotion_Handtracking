using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierPackageReceiverTCP))]
public class BezierPackageReceiverTCPInspector : Editor {

	private BezierPackageReceiverTCP receiver;

	private void OnSceneGUI() {
		//receiver = target as UDPReceive;
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		receiver = target as BezierPackageReceiverTCP;

		GUILayout.Space (20);
		GUILayout.Label ("Status : " + receiver.Status());
		if (receiver.networkReceiver != null) {
			/*EditorGUI.BeginChangeCheck ();
			int port = EditorGUILayout.IntField ("Port", receiver.port);
			if (EditorGUI.EndChangeCheck ()) {
				receiver.port = port;
				EditorUtility.SetDirty (receiver);
			}*/
			GUILayout.Label ("Thread running : " + (receiver.networkReceiver.IsThreadRunning ? "Yes" : "No"));
			if (GUILayout.Button ("Listen")) {
				receiver.Start ();
				EditorUtility.SetDirty (receiver);
			}
			if (receiver.networkReceiver.IsListening && GUILayout.Button ("Stop")) {
				receiver.Stop ();
				EditorUtility.SetDirty (receiver);
			}
		}
		GUILayout.Space (20);
		GUILayout.Label ("Messages in queue : " + receiver.MessageCount);


		BezierPackage package = receiver.PeekNextMessage ();
		if (package == null)
			package = new BezierPackage();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("First package in queue");
		EditorGUILayout.EnumPopup ("action", package.action);
		EditorGUILayout.IntField ("spline id", (int)package.splineId);
		//EditorGUILayout.IntField ("spline extrude shape", (int)package.splineExtrudeShapeType);
		EditorGUILayout.Vector3Field ("position", package.pointPosition);

		Repaint ();
	}
}