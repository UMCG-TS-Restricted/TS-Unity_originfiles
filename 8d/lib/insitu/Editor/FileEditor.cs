using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace insitu
{
	public class FileEditor : EditorWindow
	{
		public App App;

		[MenuItem("Window/insitu/File Editor")]
		public static FileEditor Open()
		{
			FileEditor wnd = GetWindow<FileEditor>();
			wnd.titleContent = new GUIContent("insitu - File Editor");
			return wnd;
		}

		public static string Save(App app)
		{
			var dir = Directory.CreateDirectory(Application.dataPath + "/../recordings");
			var telemetry = app.Telemetry;
			var now = DateTime.Now;
			var str = "rec_" + now.ToString("yyyyMMddHHmmss");
			var path = EditorUtility.SaveFilePanel("Save Recording", dir.FullName, str, "bytes");
			telemetry.Save(path, app.Settings.CloneObject());
			return path;
		}

		public static bool Load(App app)
		{
			if (!app)
				return false;

			var dir = Directory.CreateDirectory(Application.dataPath + "/../recordings");
			var path = EditorUtility.OpenFilePanel("Load Recording", dir.FullName, "bytes");
			if (string.IsNullOrEmpty(path))
				return false;

			var file = Telemetry.File.Read(path);
			if (file.blocks.length == 0)
				return false;

			app.LoadedFiles = app.LoadedFiles.Append(file);
			return true;
		}



		public void OnGUI()
		{
			App = (App)EditorGUILayout.ObjectField("App", App, typeof(App), false);
			if (!App)
			{
				EditorGUILayout.HelpBox(App.NotAssigned, MessageType.Warning);
				return;
			}

			var files = App.LoadedFiles;

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Open"))
			{
				if (files.length > 0)
				{
					var clear = EditorUtility.DisplayDialog(
					"insitu - Opening new file",
					"Are you sure to open a new file while a file is already been loaded? " +
					"Continueing means losing any changes made to the loaded file.",
					"Continue",
					"Cancel");

					if (clear)
					{
						files.length = 0;
						App.LoadedFiles = files;
					}
				}

				if (files.length == 0)
				{
					Load(App);
					files = App.LoadedFiles;
				}

			}

			GUI.enabled = files.length > 0;
			if (GUILayout.Button("Save"))
			{
				
			}

			GUI.enabled = files.length > 0;
			if (GUILayout.Button("Save As.."))
			{
				
			}

			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();





			if (files.length < 1)
			{
				EditorGUILayout.HelpBox(App.FileIsNull, MessageType.Warning);
				return;
			}

			var file = files[0];




		}
	}

}
