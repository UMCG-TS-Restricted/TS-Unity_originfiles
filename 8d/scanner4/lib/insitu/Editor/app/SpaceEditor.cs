using ADG;
using UnityEditor;
using UnityEngine;


namespace insitu
{
	[CustomEditor(typeof(Space))]
	public class SpaceEditor : Editor
	{
		public Json.Object Settings;

		public void OnEnable()
		{
			var space = (Space)target;
			var app = space.App;
			if (app) app.LoadSettings();
		}

		public static void Check(PoseBehaviour pose, string name)
		{
			if (!pose)
				EditorGUILayout.HelpBox($"{name} is not assigned", MessageType.Error);
			else if (pose is not IPoseSource)
				EditorGUILayout.HelpBox($"{name} is assigned, but only has access to processed data. Consider using a type that implements IPoseSource.", MessageType.Warning);
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("This component can be used to align the Vicon axis with the Unity axis. Place the callibration wand on the center position of which the player will be standing. Queue a snapshot to apply the transformation matrix. If Save Snapshot is enabled, the transformation matrix and center position will be saved to the app settings.", MessageType.Info);
			base.OnInspectorGUI();

			var space = (Space)target;
			var app = space.App;
			var settings = app ? app.FetchSettings() : null;
			GUI.enabled = settings != null && settings.ContainsKey("space");
			if (GUILayout.Button("Delete Saved Transform Space"))
			{
				settings.Remove("space");
				App.Save(settings);
			}
			GUI.enabled = app && space.Left && space.Center && space.Back;
			if (GUILayout.Button("Create and Save Transform Space"))
				space.Snap(true);

			if (!app)
				EditorGUILayout.HelpBox(error.AppNotAssigned, MessageType.Error);

			Check(space.Left, "Left");
			Check(space.Center, "Center");
			Check(space.Back, "Back");
		}
	}
}
