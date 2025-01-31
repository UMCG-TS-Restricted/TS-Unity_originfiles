using UnityEditor;
using ADG;


namespace insitu
{
	[CustomEditor(typeof(UnityStateDevice))]
	public class UnityStateDeviceEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = (UnityStateDevice)target;
			var app = obj.App;
			if (UnityStateEditor.ErrorOf(app))
				return;

			if (obj.Index < 0)
			{
				EditorGUILayout.HelpBox(error.DeviceNotFound, MessageType.None);
				return;
			}

			var json = obj.Device.ToJson(obj.Output);
			EditorGUILayout.HelpBox(json.Stringify(Json.Pretty), MessageType.None);
		}
	}
}
