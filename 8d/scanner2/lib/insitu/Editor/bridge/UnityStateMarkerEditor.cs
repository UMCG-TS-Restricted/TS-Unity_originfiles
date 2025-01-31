using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityStateMarker))]
	public class UnityStateMarkerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateMarker)target;
			var app = obj.App;
			if (UnityStateEditor.ErrorOf(app))
				return;

			var json = obj.Marker.ToJson();
			EditorGUILayout.HelpBox(json.Stringify(Json.Pretty), MessageType.None);
		}
	}
}
