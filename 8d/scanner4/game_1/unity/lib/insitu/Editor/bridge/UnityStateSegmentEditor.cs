using UnityEngine;
using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityStateSegment))]
	public class UnityStateSegmentEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateSegment)target;
			var app = obj.App;
			var error = UnityStateEditor.ErrorOf(app);
			if (!app)
				return;

			var settings = app.FetchSettings();
			Json.Array reference = null;
			string key = null;

			var parent = settings.ObjectOf("segments");
			if (parent != null)
			{
				key = $"{obj.Subject.Value}:{obj.Name}";
				var self = parent.ObjectOf(key);
				if (self != null)
					reference = self.ArrayOf("reference");
			}

			GUI.enabled = reference != null;
			if (GUILayout.Button("Clear reference point"))
			{
				parent.Remove(key);
				App.Save(settings);
			}
			GUI.enabled = !error;
			if (GUILayout.Button("Calculate and save reference point"))
			{
				if (obj.CreateReference(3))
				{
					obj.StoreReference(settings);
					App.Save(settings);
				}
				else Debug.LogError("Failed to create the reference!");
			}
			GUI.enabled = true;
			if (error)
				return;

			var json = obj.Segment.ToJson();
			EditorGUILayout.HelpBox(json.Stringify(Json.Pretty), MessageType.None);
		}
	}
}
