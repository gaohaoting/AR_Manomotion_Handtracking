using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierPackageSenderUDP))]
public class BezierPackageSenderUDPInspector : Editor {

	private BezierPackageSenderUDP sender;

	private void OnSceneGUI() {
		//sender = target as UDPSend;
	}

	public override void OnInspectorGUI() {
		
		//DrawDefaultInspector ();

		sender = target as BezierPackageSenderUDP;

		/*EditorGUI.BeginChangeCheck ();
		UDPSender networkSender = (UDPSender)EditorGUILayout.ObjectField ("Network Sender", sender.networkSender, typeof(UDPSender), true);
		if (EditorGUI.EndChangeCheck ()) {
			sender.networkSender = networkSender;
			EditorUtility.SetDirty (sender);
		}*/
		EditorGUI.BeginChangeCheck ();
		string ip = EditorGUILayout.TextField ("IP", sender.ip);
		if (EditorGUI.EndChangeCheck ()) {
			sender.ip = ip;
			EditorUtility.SetDirty (sender);
		}
		EditorGUI.BeginChangeCheck ();
		int port = EditorGUILayout.IntField ("Port", sender.port);
		if (EditorGUI.EndChangeCheck ()) {
			sender.port = port;
			EditorUtility.SetDirty (sender);
		}
		GUILayout.Label ("Status : " + sender.Status());
		if ((sender.IsConnected()) && GUILayout.Button ("Disconnect")) {
			sender.Disconnect ();
			EditorUtility.SetDirty (sender);
		}
		if ((!sender.IsConnected()) && GUILayout.Button ("Connect")) {
			sender.Connect ();
			EditorUtility.SetDirty (sender);
		}
		GUILayout.Label ("Simple Send Counter : " + sender.simpleSendCounter);


        GUILayout.Space(20);
        GUILayout.Label("Things to send (apart from scene manager):");
        EditorGUI.BeginChangeCheck();
        Transform cameraTransform = (Transform)EditorGUILayout.ObjectField("Camera Transform", sender.cameraTransform, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            sender.cameraTransform = cameraTransform;
            EditorUtility.SetDirty(sender);
        }
        EditorGUI.BeginChangeCheck();
        Transform leftControllerTransform = (Transform)EditorGUILayout.ObjectField("Left Controller Transform", sender.leftControllerTransform, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            sender.leftControllerTransform = leftControllerTransform;
            EditorUtility.SetDirty(sender);
        }
        EditorGUI.BeginChangeCheck();
        Transform rightControllerTransform = (Transform)EditorGUILayout.ObjectField("Right Controller Transform", sender.rightControllerTransform, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            sender.rightControllerTransform = rightControllerTransform;
            EditorUtility.SetDirty(sender);
        }
        EditorGUI.BeginChangeCheck();
        Settings settings = (Settings)EditorGUILayout.ObjectField("Settings", sender.settings, typeof(Settings), true);
        if (EditorGUI.EndChangeCheck())
        {
            sender.settings = settings;
            EditorUtility.SetDirty(sender);
        }


        GUILayout.Space (20);
		//GUILayout.Label ("Save to file");
		EditorGUI.BeginChangeCheck ();
		BezierPackageRecorder recorder = (BezierPackageRecorder)EditorGUILayout.ObjectField ("Recorder", sender.recorder, typeof(BezierPackageRecorder), true);
		if (EditorGUI.EndChangeCheck ()) {
			sender.recorder = recorder;
			EditorUtility.SetDirty (sender);
		}

		// Forces repainting the inspector while it is selected
		Repaint ();
	}
}