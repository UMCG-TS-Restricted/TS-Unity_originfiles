using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Button))]
public class ButtonEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		var button = (Button)target;
		GUI.enabled = EditorApplication.isPlaying;
		if (GUILayout.Button("Click"))
			button.FakeClick();
	}
}
