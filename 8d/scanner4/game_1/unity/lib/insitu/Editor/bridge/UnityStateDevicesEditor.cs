using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityStateDevices))]
	public class UnityStateDevicesEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateDevices)target;
			var app = obj.App;
			if (UnityStateEditor.ErrorOf(app))
				return;

			EditorGUILayout.HelpBox("Go to the object's children to get details of the devices.", MessageType.Info);
		}
	}
}
