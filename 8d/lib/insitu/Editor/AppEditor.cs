using System.IO;
using ADG;
using UnityEditor;


namespace insitu
{
	[CustomEditor(typeof(App))]
	public class AppEditor : Editor
	{
		public static readonly string[] ViconModes =
		{
			"None",
			"Nexus",
			"Tracker",
		};

		public string SettingsString;
		public Json.Object Settings;

		public void OnEnable()
		{
			var settings = App.Load(false);
			Settings = settings;
			SettingsString = settings.Stringify(Json.Pretty);
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();
			var settings = Settings;
			if (settings == null)
				settings = Settings = new Json.Object();

			var host = EditorGUILayout.TextField("IP Address", settings["host"]);
			var mode = EditorGUILayout.Popup("Vicon Mode", (int)settings["mode"], ViconModes);
			if (EditorGUI.EndChangeCheck())
			{
				settings["host"] = host;
				settings["mode"] = mode;
				var content = settings.Stringify(Json.Pretty);
				SettingsString = content;
				File.WriteAllText(App.EditorSettingsPath, content);
			}

			EditorGUILayout.Space();
			EditorGUILayout.TextArea(SettingsString);
		}
	}
}
