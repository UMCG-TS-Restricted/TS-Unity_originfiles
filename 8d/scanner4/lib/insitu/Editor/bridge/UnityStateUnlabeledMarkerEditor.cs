using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityStateUnlabeledMarker))]
	public class UnityStateUnlabeledMarkerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateUnlabeledMarker)target;
			var app = obj.App;
			if (UnityStateEditor.ErrorOf(app))
				return;

			var json = obj.Marker.ToJson();
			EditorGUILayout.HelpBox(json.Stringify(Json.Pretty), MessageType.None);
		}
	}
}
