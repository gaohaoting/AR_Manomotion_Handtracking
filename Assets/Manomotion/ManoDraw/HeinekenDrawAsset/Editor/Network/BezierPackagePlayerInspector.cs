using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(BezierPackagePlayer))]
public class BezierPackagePlayerInspector : Editor {

	private BezierPackagePlayer player;

	private string lastFileFolder = "~/Desktop";

	public override void OnInspectorGUI() {

		player = target as BezierPackagePlayer;

		EditorGUI.BeginChangeCheck();
		OscOut oscOut = (OscOut)EditorGUILayout.ObjectField("oscOut", player.oscOut, typeof(OscOut), true);
		if (EditorGUI.EndChangeCheck())
		{
			player.oscOut = oscOut;
			EditorUtility.SetDirty(player);
		}
		EditorGUI.BeginChangeCheck();
		BezierPackageSenderUDP udpSender = (BezierPackageSenderUDP)EditorGUILayout.ObjectField("UDP Sender", player.udpSender, typeof(BezierPackageSenderUDP), true);
		if (EditorGUI.EndChangeCheck())
		{
			player.udpSender = udpSender;
			EditorUtility.SetDirty(player);
		}

		GUILayout.Space(20);
        EditorGUI.BeginChangeCheck();
        WandPackage wand = (WandPackage)EditorGUILayout.ObjectField("Wand [WandPackage]", player.wandPackage, typeof(WandPackage), true);
        if (EditorGUI.EndChangeCheck())
        {
            player.wandPackage = wand;
            EditorUtility.SetDirty(player);
        }

        EditorGUI.BeginChangeCheck ();
		BezierSplineSceneManager sceneManager = (BezierSplineSceneManager)EditorGUILayout.ObjectField ("Scene Manager", player.sceneManager, typeof(BezierSplineSceneManager), true);
		if (EditorGUI.EndChangeCheck ()) {
            player.sceneManager = sceneManager;
			EditorUtility.SetDirty (player);
		}

        GUILayout.Space(20);
        EditorGUI.BeginChangeCheck();
        Transform cameraTransform = (Transform)EditorGUILayout.ObjectField("Camera Transform", player.cameraTransform, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            player.cameraTransform = cameraTransform;
            EditorUtility.SetDirty(player);
        }
        EditorGUI.BeginChangeCheck();
        Transform leftControllerTransform = (Transform)EditorGUILayout.ObjectField("Left Controller Transform", player.leftControllerTransform, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            player.leftControllerTransform = leftControllerTransform;
            EditorUtility.SetDirty(player);
        }
        EditorGUI.BeginChangeCheck();
        Transform rightControllerTransform = (Transform)EditorGUILayout.ObjectField("Right Controller Transform", player.rightControllerTransform, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            player.rightControllerTransform = rightControllerTransform;
            EditorUtility.SetDirty(player);
        }


        GUILayout.Space(20);
        EditorGUI.BeginChangeCheck();
        string filePath = EditorGUILayout.TextField("File path", player.filePath, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
        if (EditorGUI.EndChangeCheck())
        {
            lastFileFolder = Path.GetDirectoryName(filePath);
            player.filePath = filePath;
            EditorUtility.SetDirty(player);
        }

        if (GUILayout.Button("Select file"))
        {
            filePath = EditorUtility.OpenFilePanel("Select file to play", lastFileFolder, "");
            if (filePath != null && filePath.Length > 0)
            {
                lastFileFolder = Path.GetDirectoryName(filePath);
                player.filePath = filePath;
                EditorUtility.SetDirty(player);
            }
        }

        EditorGUI.BeginChangeCheck();
        bool startDirectly = EditorGUILayout.Toggle("Start Playing Directly", player.startPlayingDirectly);
        if (EditorGUI.EndChangeCheck())
        {
            player.startPlayingDirectly = startDirectly;
            EditorUtility.SetDirty(player);
        }

        EditorGUI.BeginChangeCheck();
        bool repeatWhenDone = EditorGUILayout.Toggle("Repeat When Done", player.repeatWhenDone);
        if (EditorGUI.EndChangeCheck())
        {
            player.repeatWhenDone = repeatWhenDone;
            EditorUtility.SetDirty(player);
        }

        EditorGUI.BeginChangeCheck();
        bool undoAllOnRepeat = EditorGUILayout.Toggle("Undo All On Repeat", player.undoAllOnRepeat);
        if (EditorGUI.EndChangeCheck())
        {
            player.undoAllOnRepeat = undoAllOnRepeat;
            EditorUtility.SetDirty(player);
        }

        EditorGUI.BeginChangeCheck();
        float playSpeed = EditorGUILayout.FloatField("Play Speed", player.playSpeed);
        if (EditorGUI.EndChangeCheck())
        {
            player.playSpeed = playSpeed;
            EditorUtility.SetDirty(player);
        }

        EditorGUI.BeginChangeCheck();
        float playFromTime = EditorGUILayout.FloatField("Play From Time", player.playFromTime);
        if (EditorGUI.EndChangeCheck())
        {
            player.playFromTime = playFromTime;
            EditorUtility.SetDirty(player);
        }

        EditorGUI.BeginChangeCheck();
        float playToTime = EditorGUILayout.FloatField("Play To Time", player.playToTime);
        if (EditorGUI.EndChangeCheck())
        {
            player.playToTime = playToTime;
            EditorUtility.SetDirty(player);
        }

        EditorGUI.BeginChangeCheck();
        bool sortData = EditorGUILayout.Toggle("Sort Data On Play", player.sortDataByTimeOnPlay);
        if (EditorGUI.EndChangeCheck())
        {
            player.sortDataByTimeOnPlay = sortData;
            EditorUtility.SetDirty(player);
        }
			
		if (!player.isPlaying && player.PathOk() && GUILayout.Button ("Play file")) {
			player.PlayFile ();
			EditorUtility.SetDirty (player);
        }

        if (player.isPlaying && GUILayout.Button("Stop Playing"))
        {
            player.StopPlayingFile();
            EditorUtility.SetDirty(player);
        }

        if (!player.isPlaying && GUILayout.Button("Clear Scene Manager"))
        {
            player.ClearSceneManager();
            EditorUtility.SetDirty(player);
        }

        if (player.isPlaying) {
			GUILayout.Label ("Time : " + player.timePlayer);
            GUILayout.Label("Next Package Time : " + player.NextPackageTime());
            GUILayout.Label ("Packages remaining : " + player.packagesRemaining);
			EditorUtility.SetDirty (player);
		}
	}
}
