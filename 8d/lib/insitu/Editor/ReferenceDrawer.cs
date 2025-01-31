using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace insitu
{
	[CustomPropertyDrawer(typeof(Reference))]
	public sealed class ReferenceDrawer : PropertyDrawer
	{
		const float height = 18;
		const float margin = 2;

		static readonly string[] Options = new string[]
		{
			"None",
			"Markers",
		};

		List<App> Apps;
		Vicon.Worker Current;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (Apps == null)
			{
				Apps = new List<App>();
				Unity.FindResources(Apps);
			}

			Vicon.Worker worker = null;
			for (var i = 0; i < Apps.Count; i++)
			{
				var app = Apps[i];
				if (app && app.Worker != null)
				{
					worker = app.Worker;
					break;
				}
			}

			EditorGUI.BeginProperty(position, GUIContent.none, property);
			var rhs = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var type_field = property.FindPropertyRelative("Type");
			var subject_field = property.FindPropertyRelative("Subject");
			var names_field = property.FindPropertyRelative("Names");

			var type_rect = rhs;
			type_rect.y += (height + margin) * 0;
			type_rect.height = height;

			var subject_rect = position;
			subject_rect.x += height;
			subject_rect.width -= height;
			subject_rect.y += (height + margin) * 1;
			subject_rect.height = height;

			var names_rect = position;
			names_rect.x += height;
			names_rect.width -= height;
			names_rect.y += (height + margin) * 2;
			names_rect.height = names_field.arraySize * height;


			EditorGUI.BeginChangeCheck();
			type_field.intValue = EditorGUI.Popup(type_rect, type_field.intValue, Options);
			switch (type_field.intValue)
			{
				case Reference.TypeMarkers:
				{
					var color = GUI.color;
					if (worker != null)
					{
						var state = worker.State;
						if (state.subjects.length > 0)
						{
							var found = false;
							for (var i = 0; i < state.subjects.length; i++)
							{
								var subject = state.subjects[i];
								if (subject.name == subject_field.stringValue)
								{
									found = true;
									break;
								}
							}

							if (!found)
								GUI.color = new Color(1.0f, 0.6f, 0.4f, 1.0f);
						}
					}

					subject_field.stringValue = EditorGUI.TextField(subject_rect, "Subject", subject_field.stringValue);
					GUI.color = color;


					EditorGUI.PropertyField(names_rect, names_field);
				} break;
			}

			if (EditorGUI.EndChangeCheck())
			{

			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var type_field = property.FindPropertyRelative("Type");
			switch (type_field.intValue)
			{
				case Reference.TypeMarkers:
				{
					var names_field = property.FindPropertyRelative("Names");
					return EditorGUI.GetPropertyHeight(names_field) + 3 * (height + margin);
				}
			}
			return height + margin;
		}
	}
}
