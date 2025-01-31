using UnityEngine;
using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityStateSubject))]
	public class UnityStateSubjectEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateSubject)target;
			var app = obj.App;
			if (UnityStateEditor.ErrorOf(app))
				return;

			var json = obj.Subject.ToJson(default, default);
			EditorGUILayout.HelpBox(json.Stringify(Json.Pretty), MessageType.None);
			EditorGUILayout.HelpBox("Go to the object's children to get details of the subject.", MessageType.Info);
		}
	}
}
