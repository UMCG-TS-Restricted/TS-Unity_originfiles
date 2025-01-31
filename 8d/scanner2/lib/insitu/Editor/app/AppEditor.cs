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
			//"Tracker",
		};

		public static readonly string[] StreamingModes =
		{
			"ClientPull",
			"ClientPullPreFetch",
			"ServerPush",
		};


		public string SettingsString;
		public bool FoldoutSettingsContent;
		public SerializedObject SerializedObject;

		public virtual void OnEnable()
		{
			var app = (App)target;
			var settings = app.LoadSettings();
			SettingsString = settings.Stringify(Json.Pretty);
		}

		public virtual void OnInspectorGUIBegin() => base.OnInspectorGUI();
		public virtual void OnInspectorGUISettings()
		{
			var app = (App)target;
			var settings = app.FetchSettings();

			EditorGUI.BeginChangeCheck();
			settings["host"] = EditorGUILayout.TextField("Host", settings.StringOf("host", "127.0.0.1"));
			settings["port"] = EditorGUILayout.IntField("Port", settings.NumberOf("port", 801));
			settings["streaming_mode"] = EditorGUILayout.Popup("Vicon Mode", settings.NumberOf("streaming_mode", 1), StreamingModes);
			settings["vicon_mode"] = EditorGUILayout.Popup("Vicon Mode", settings.NumberOf("vicon_mode", 1), ViconModes);
			settings["scale"] = EditorGUILayout.FloatField("Scale", settings.NumberOf("scale", 1));

			if (EditorGUI.EndChangeCheck())
			{
				var content = settings.Stringify(Json.Pretty);
				SettingsString = content;
				File.WriteAllText(App.SettingsPath, content);
			}
		}
		public virtual void OnInspectorGUIEnd(Vicon.State state) { }

		public override void OnInspectorGUI()
		{
			var app = (App)target;
			OnInspectorGUIBegin();
			OnInspectorGUISettings();
			FoldoutSettingsContent = EditorGUILayout.Foldout(FoldoutSettingsContent, "Settings File Contents");
			if (FoldoutSettingsContent)
			{
				EditorGUILayout.HelpBox(SettingsString, MessageType.None);
			}
			
			if (!app)
				return;

			EditorGUILayout.Space();
			var worker = app.Worker;
			if (worker == null)
			{
				EditorGUILayout.HelpBox(error.WorkerIsNull, MessageType.Warning);
				return;
			}

			var state = worker.State;
			if (state.version == 0)
			{
				EditorGUILayout.HelpBox(error.StateIsNull, MessageType.Warning);
				return;
			}

			OnInspectorGUIEnd(state);
		}
	}
}
