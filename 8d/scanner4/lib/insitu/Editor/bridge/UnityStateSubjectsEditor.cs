using UnityEngine;
using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityStateSubjects))]
	public class UnityStateSubjectsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateSubjects)target;
			var app = obj.App;
			if (UnityStateEditor.ErrorOf(app))
				return;

			EditorGUILayout.HelpBox("Go to the object's children to get details of the subjects.", MessageType.Info);
		}
	}
}
