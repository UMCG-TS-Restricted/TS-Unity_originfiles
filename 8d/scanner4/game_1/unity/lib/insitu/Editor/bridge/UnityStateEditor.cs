using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityState))]
	public class UnityStateEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityState)target;
			if (obj.enabled)
			{
				var app = obj.App;
				if (ErrorOf(app))
					return;

				var json = obj.State.ToJson(false);
				EditorGUILayout.HelpBox(json.Stringify(Json.Pretty), MessageType.None);
			}
			else
			{
				var json = obj.State.ToJson(false);
				EditorGUILayout.HelpBox(json.Stringify(Json.Pretty), MessageType.None);
			}
		}

		public static bool ErrorOf(App app)
		{
			if (!app)
			{
				EditorGUILayout.HelpBox(error.AppNotAssigned, MessageType.Error);
				return true;
			}

			var worker = app.Worker;
			if (worker == null)
			{
				EditorGUILayout.HelpBox(error.WorkerIsNull, MessageType.Warning);
				return true;
			}

			var state = worker.State;
			if (state.version < 0)
			{
				EditorGUILayout.HelpBox(error.StateIsDead, MessageType.Warning);
				return true;
			}
			if (state.version == 0)
			{
				EditorGUILayout.HelpBox(error.StateIsNull, MessageType.Info);
				return true;
			}

			return false;
		}
	}
}
