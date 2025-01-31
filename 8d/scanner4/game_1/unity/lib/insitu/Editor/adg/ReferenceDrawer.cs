using UnityEditor;
using UnityEngine;


namespace ADG
{
	[CustomPropertyDrawer(typeof(StringReference))]
	public class ReferenceDrawer : PropertyDrawer
	{
		const float ToggleWidth = ToggleControlWidth + ToggleLabelWidth;
		const float ToggleControlWidth = 20;
		const float ToggleLabelWidth = 65;
		const float TogglePadding = 20;

		/// <inheritdoc />
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var propertyRect = position;
			propertyRect.xMax -= ToggleWidth;
			var shared = property.FindPropertyRelative("UseShared");
			var name = shared.boolValue ? "Shared" : "Self";
			EditorGUI.PropertyField(propertyRect, property.FindPropertyRelative(name), label);
			position.xMin = propertyRect.xMax + TogglePadding;
			propertyRect = position;
			propertyRect.width = ToggleControlWidth;
			EditorGUI.PropertyField(propertyRect, shared, GUIContent.none);
			position.xMin = propertyRect.xMax;
			propertyRect = position;
			EditorGUI.LabelField(propertyRect, "Shared");
			EditorGUI.EndProperty();
		}
	}
}
