using System;
using System.IO;
using insitu;
using UnityEditor;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using Json = ADG.Json;


[CustomEditor(typeof(Main))]
public class FileEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		GUILayout.Space(28);

		var main = (Main)target;
		var app  = main.App;
		if (!app)
		{
			EditorGUILayout.HelpBox(error.AppNotAssigned, MessageType.Warning);
			return;
		}

		var files = FindObjectsOfType<Playback>(false);


		GUI.enabled = app && EditorApplication.isPlaying;
		if (main.RecordingState == Main.RecordNone)
		{
			if (GUILayout.Button("Start Recording"))
				main.RecordingState = Main.RecordStart;
		}
		else if (main.RecordingState == Main.RecordStart)
		{
			GUI.enabled = false;
			GUILayout.Button("Starting..");
		}
		else if (main.RecordingState == Main.RecordRunning)
		{
			if (GUILayout.Button("Stop"))
			{
				main.RecordingState = Main.RecordStop;
				Save(app, main);
				main.RecordingState = Main.RecordNone;
			}
		}


		EditorGUILayout.BeginHorizontal();
		GUI.enabled = main.transform && app.PlaybackAsset;
		var open_file = GUILayout.Button("Open");

		GUI.enabled = files.Length > 0 && files[0].File.data != null;
		var json_export = GUILayout.Button("JSON Export..");
		EditorGUILayout.EndHorizontal();


		if (open_file)
		{
			if (files.Length > 0)
			{
				var clear = EditorUtility.DisplayDialog(
					"insitu - Opening new file",
					"Are you sure to open a new file while a file is already been loaded? " +
					"Continueing means losing any changes made to the loaded file.",
					"Continue",
					"Cancel");

				if (clear)
				{
					for (var i = 0; i < files.Length; i++)
						Destroy(files[i].gameObject);
				}
			}

			if (files == null || files.Length == 0)
			{
				Load(app, main.transform);
				files = FindObjectsOfType<Playback>(false);
			}
		}

		if (json_export)
		{
			var file = files[0];
			var dir = Directory.CreateDirectory(Application.dataPath + "/../recordings");
			var file_info = new FileInfo(file.FilePath);
			var name = file_info.Name;
			var extension = file_info.Extension;
			name = name.Substring(0, name.Length - extension.Length);

			var path = EditorUtility.SaveFilePanel("Save Recording", dir.FullName, name, "json");
			if (!string.IsNullOrEmpty(path))
			{
				var json = Playback.ToJson(file.File);
				var text = json.Stringify(Json.Pretty);
				File.WriteAllText(path, text);
			}
		}



		GUI.enabled = true;
		if (files.Length < 1)
		{
			EditorGUILayout.HelpBox(error.FileIsNull, MessageType.Info);
			return;
		}

		var playback = files[0];
		var obj = new SerializedObject(playback);
		var target_time = obj.FindProperty("TargetTime");

		GUI.enabled = playback.Range(out var min_time, out var max_time);
		GUILayout.Label("Time");
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Slider(target_time, min_time, max_time);
		if (EditorGUI.EndChangeCheck())
		{
			obj.ApplyModifiedProperties();
		}
	}

	public static string Save(App app, Main target)
	{
		var dir = Directory.CreateDirectory(Application.dataPath + "/../recordings");
		var telemetry = app.Telemetry;
		var now = DateTime.Now;
		var str = "rec_" + now.ToString("yyyyMMddHHmmss");
		var path = EditorUtility.SaveFilePanel("Save Recording", dir.FullName, str, "bytes");
		if (!string.IsNullOrEmpty(path))
		{
			lock (target)
			{
				telemetry.Save(path, app.Settings.CloneObject());
			}

		}
		return path;
	}

	public static bool Load(App app, Transform parent)
	{
		var dir = Directory.CreateDirectory(Application.dataPath + "/../recordings");
		var path = EditorUtility.OpenFilePanel("Load Recording", dir.FullName, "bytes");
		if (string.IsNullOrEmpty(path))
			return false;

		var file = insitu.telemetry.File.Read(path);
		if (file.blocks.length == 0)
			return false;

		var instance = Instantiate(app.PlaybackAsset, parent);
		instance.File = file;
		//instance.gameObject.hideFlags = HideFlags.DontSave;

		var obj = new SerializedObject(instance);
		var file_path = obj.FindProperty("FilePath");
		file_path.stringValue = path;

		if (file.frames.length > 0)
		{
			var target_time = obj.FindProperty("TargetTime");
			target_time.floatValue = file.frames[0].time;
		}

		obj.ApplyModifiedPropertiesWithoutUndo();
		return true;
	}
}