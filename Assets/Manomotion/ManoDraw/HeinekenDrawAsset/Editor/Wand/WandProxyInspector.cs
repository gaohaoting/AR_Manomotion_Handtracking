using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrawingWand))]
public class WandProxyInspector : WandBaseInspector
{

    private DrawingWand wandProxy;

    public override void OnInspectorGUI()
    {

        wandProxy = target as DrawingWand;

        GUILayout.Space(20);
        GUILayout.Label("Wand Base Inspector");
        //base.OnInspectorGUI ();
        Settings settings = Settings.GetInstance();
        string[] brushNames = settings.GetListOfBrushNames();

        EditorGUI.BeginChangeCheck();
        int selectedBrush = EditorGUILayout.Popup("Brush", wandProxy.selectedBrush, brushNames);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.selectedBrush = selectedBrush;
            EditorUtility.SetDirty(wandProxy);
        }

        EditorGUI.BeginChangeCheck();
        BezierSplineSceneManager sceneManager = (BezierSplineSceneManager)EditorGUILayout.ObjectField("Scene Manager", wandProxy.sceneManager, typeof(BezierSplineSceneManager), true);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.sceneManager = sceneManager;
            EditorUtility.SetDirty(wandProxy);
        }

        /*EditorGUI.BeginChangeCheck ();
		BezierUDPSender udpSender = (BezierUDPSender)EditorGUILayout.ObjectField ("UDP Sender", wandProxy.udpSender, typeof(BezierUDPSender), true);
		if (EditorGUI.EndChangeCheck ()) {
			wandProxy.udpSender = udpSender;
			EditorUtility.SetDirty (wandProxy);
		}*/

        GUILayout.Label("Min Sample Distance : " + wandProxy.minSampleDistance);




        GUILayout.Space(20);
        GUILayout.Label("Wand Proxy Inspector");

        EditorGUI.BeginChangeCheck();
        Transform transform = (Transform)EditorGUILayout.ObjectField("Controller Transform", wandProxy.controllerTransform, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.controllerTransform = transform;
            EditorUtility.SetDirty(wandProxy);
        }

        EditorGUI.BeginChangeCheck();
        float widthPostClampScaleFactor = EditorGUILayout.FloatField("Width Post-Clamp Scale Factor", wandProxy.widthPostClampScaleFactor);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.widthPostClampScaleFactor = widthPostClampScaleFactor;
            EditorUtility.SetDirty(wandProxy);
        }

        EditorGUI.BeginChangeCheck();
        float customParameter = EditorGUILayout.FloatField("Custom Parameter", wandProxy.customParameter);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.customParameter = customParameter;
            EditorUtility.SetDirty(wandProxy);
        }

        EditorGUI.BeginChangeCheck();
        bool drawDebug = EditorGUILayout.Toggle("Draw Debug", wandProxy.drawWandDebug);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.drawWandDebug = drawDebug;
            EditorUtility.SetDirty(wandProxy);
        }
        EditorGUI.BeginChangeCheck();
        Vector3 offsetPosition = EditorGUILayout.Vector3Field("Offset Position", wandProxy.offsetPosition);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.offsetPosition = offsetPosition;
            EditorUtility.SetDirty(wandProxy);
        }
        EditorGUI.BeginChangeCheck();
        Vector3 offsetRotation = EditorGUILayout.Vector3Field("Offset Rotation", wandProxy.offsetRotation.eulerAngles);
        if (EditorGUI.EndChangeCheck())
        {
            wandProxy.offsetRotation.eulerAngles = offsetRotation;
            EditorUtility.SetDirty(wandProxy);
        }

        if (GUILayout.Button("Undo Down"))
        {
            wandProxy.undoButtonDown = true;
            EditorUtility.SetDirty(wandProxy);
        }
        if (GUILayout.Button("Undo All Down"))
        {
            wandProxy.undoAllButtonDown = true;
            EditorUtility.SetDirty(wandProxy);
        }
        if (GUILayout.Button("Redo Down"))
        {
            wandProxy.redoButtonDown = true;
            EditorUtility.SetDirty(wandProxy);
        }

        if (GUILayout.Button("Draw Button Down"))
        {
            wandProxy.drawButtonDown = true;
            EditorUtility.SetDirty(wandProxy);
        }

        if (GUILayout.Button("Draw Button Up"))
        {
            wandProxy.drawButtonUp = true;
            EditorUtility.SetDirty(wandProxy);
        }
    }
}