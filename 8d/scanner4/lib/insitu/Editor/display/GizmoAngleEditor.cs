using UnityEditor;
using ADG;
using UnityEngine;
using System.Collections.Generic;


namespace insitu
{
	[CustomEditor(typeof(GizmoAngle))]
	public class GizmoAngleEditor : Editor
	{
		public List<GizmoData> Cache;

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("", MessageType.Info);
			base.OnInspectorGUI();

			var has_error = false;
			var gizmo = (GizmoAngle)target;
			var playing = EditorApplication.isPlaying;
			if (!gizmo.Data)
			{
				if (Cache == null)
					Cache = new List<GizmoData>();
				EditorUtil.FindAll(Cache);
				if (Cache.Count > 0)
				{
					var first = Cache[0];
					var property = serializedObject.FindProperty("Data");
					property.objectReferenceValue = first;
					serializedObject.ApplyModifiedProperties();
				}
				else EditorGUILayout.HelpBox(error.FieldIsNull("Data"), MessageType.Error);
			}

			if (!gizmo.PoseStart)
			{
				has_error = true;
				EditorGUILayout.HelpBox(error.FieldIsNull("PoseStart"), MessageType.Error);
			}
			else if (playing && gizmo.PoseStart.Pose().valid_position == 0)
			{
				has_error = true;
				EditorGUILayout.HelpBox(error.InvalidPose("PoseStart"), MessageType.Warning);
			}

			if (!gizmo.PoseCenter)
			{
				has_error = true;
				EditorGUILayout.HelpBox(error.FieldIsNull("PoseCenter"), MessageType.Error);
			}
			else if (playing && gizmo.PoseCenter.Pose().valid_position == 0)
			{
				has_error = true;
				EditorGUILayout.HelpBox(error.InvalidPose("PoseCenter"), MessageType.Warning);
			}

			if (!gizmo.PoseEnd)
			{
				has_error = true;
				EditorGUILayout.HelpBox(error.FieldIsNull("PoseEnd"), MessageType.Error);
			}
			else if (playing && gizmo.PoseEnd.Pose().valid_position == 0)
			{
				has_error = true;
				EditorGUILayout.HelpBox(error.InvalidPose("PoseEnd"), MessageType.Warning);
			}

			if (!has_error)
			{
				GUI.enabled = false;
				var data = 

				GUI.enabled = true;
			}
		}
	}
}
