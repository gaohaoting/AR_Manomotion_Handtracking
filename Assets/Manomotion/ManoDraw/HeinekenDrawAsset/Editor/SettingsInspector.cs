using UnityEngine;
using UnityEditor;
using System;
using System.IO;

[CustomEditor(typeof(Settings))]
public class SettingsInspector : Editor {
	
	private string lastFileFolder = "/";

	public override void OnInspectorGUI() {

		DrawDefaultInspector ();

		Settings settings = target as Settings;

		/*for (int i = 0; i < settings.BrushCount; ++i) {
			DrawBrushSettings (settings.GetBrushById (i), settings);
		}*/

		GUILayout.Space (20);
		if (GUILayout.Button ("Create Brush")) {
			settings.CreateNewBrush (); 
			EditorUtility.SetDirty (settings);
        }
		if (GUILayout.Button("Export as JSON"))
		{
			string currentFileName = "BrushSettings_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
			string filePath = EditorUtility.SaveFilePanel("Select brush settings file", lastFileFolder, currentFileName, "json");
			if (filePath != null && filePath.Length > 0)
			{
				lastFileFolder = Path.GetDirectoryName(filePath);

				using (StreamWriter w = new StreamWriter(filePath))
				{
					string json = JsonUtility.ToJson (settings.GetSettingsPlainCopy());

					w.Write (json);
				}

				EditorUtility.SetDirty(settings);
			}
		}
		if (GUILayout.Button("Import from JSON"))
		{
			string filePath = EditorUtility.OpenFilePanel("Select brush settings file", lastFileFolder, "");
			if (filePath != null && filePath.Length > 0)
			{
				lastFileFolder = Path.GetDirectoryName(filePath);

				using (StreamReader r = new StreamReader(filePath))
				{
					string json = r.ReadToEnd();

					SettingsPlain sPlain = JsonUtility.FromJson<SettingsPlain> (json);
					settings.SetFromSettingsPlain (sPlain);
				}

				EditorUtility.SetDirty(settings);
			}
		}
	}

	/*
	private void DrawBrushSettings(BrushSettings brush, Settings settings) {
		if (brush.bezierSplineWidthSettings != null) {
			GUILayout.Space (20);
			GUILayout.Label ("Brush");

			EditorGUI.BeginChangeCheck ();
			string name = EditorGUILayout.TextField ("Name", brush.name);
			if (EditorGUI.EndChangeCheck ()) {
				brush.name = name;
				EditorUtility.SetDirty (settings);
			}
			EditorGUI.BeginChangeCheck ();
			Material material = (Material)EditorGUILayout.ObjectField( "Material", brush.brushMaterial, typeof(Material), false);
			if (EditorGUI.EndChangeCheck ()) {
				brush.brushMaterial = material;
				EditorUtility.SetDirty (settings);
			}
		}

		if (GUILayout.Button ("Delete Brush")) {
			settings.DeleteBrush (brush);
			EditorUtility.SetDirty (settings);
		}
	}*/
}
