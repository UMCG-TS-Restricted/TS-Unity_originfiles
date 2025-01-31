using UnityEditor;
using ADG;
using UnityEngine;
using System.Collections.Generic;


namespace insitu
{
	[CustomEditor(typeof(GizmoData))]
	public class GizmoDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("", MessageType.Info);
			base.OnInspectorGUI();

			var data = (GizmoData)target;
			if (data.Gizmos == null)
				return;

			if (data.Gizmos.Length == 0)
			{
				var cache = new List<Gizmo>();
				EditorUtil.FindAll(cache, "gizmo_");

				var property = serializedObject.FindProperty("Gizmos");
				for (var i = 0; i < cache.Count; i++)
				{
					property.InsertArrayElementAtIndex(i);
					var element = property.GetArrayElementAtIndex(i);
					element.objectReferenceValue = cache[i];
				}
			}

			if (data.GizmoMaterials.Length == 0)
			{
				var cache = new List<Material>();
				EditorUtil.FindAll(cache, "gizmo_");

				var property = serializedObject.FindProperty("GizmoMaterials");
				for (var i = 0; i < cache.Count; i++)
				{
					property.InsertArrayElementAtIndex(i);
					var element = property.GetArrayElementAtIndex(i);
					element.objectReferenceValue = cache[i];
				}
			}

			if (data.GizoTexts.Length == 0)
			{
				var cache = new List<TextComponent>();
				EditorUtil.FindAll(cache, "gizmo_");

				var property = serializedObject.FindProperty("GizoTexts");
				for (var i = 0; i < cache.Count; i++)
				{
					property.InsertArrayElementAtIndex(i);
					var element = property.GetArrayElementAtIndex(i);
					element.objectReferenceValue = cache[i];
				}
			}
		}
	}
}
