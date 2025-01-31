using UnityEditor;


namespace insitu
{
	[CustomEditor(typeof(UnityStateUnlabeledMarkers))]
	public class UnityStateUnlabeledMarkersEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateUnlabeledMarkers)target;
			var app = obj.App;
			if (UnityStateEditor.ErrorOf(app))
				return;

			EditorGUILayout.HelpBox("Go to the object's children to get details of the unlabeled markers.", MessageType.Info);
		}
	}
}
