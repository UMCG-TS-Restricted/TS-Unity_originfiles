using UnityEditor;
using UnityEngine;


namespace insitu
{
	[CustomPropertyDrawer(typeof(identifier))]
	public sealed class IdentifierDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			var value_field = property.FindPropertyRelative("value");
			if (value_field.intValue == 0)
				value_field.intValue = identifier.request();
			var value = value_field.intValue;
			var display = value.ToString("X8");
			EditorGUI.LabelField(position, display);
			EditorGUI.EndProperty();
		}
	}
}
