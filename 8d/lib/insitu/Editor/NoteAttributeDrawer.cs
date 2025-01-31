using UnityEditor;
using UnityEngine;


namespace insitu
{
	[CustomPropertyDrawer(typeof(NoteAttribute))]
	public sealed class NoteAttributeDrawer : DecoratorDrawer
	{
		/// <inheritdoc />
		public override void OnGUI(Rect position)
		{
			var note = attribute as NoteAttribute;
			var message_type = IntToMessageType(note.style);
			var content = new GUIContent(note.text);
			var style = GUI.skin.GetStyle("helpbox");
			var size = style.CalcHeight(content, position.width - note.margin_left - note.margin_right);
			if (size < note.height_min)
				size = note.height_min;

			var note_rect = new Rect(
				position.x + note.margin_left,
				position.y + note.margin_top,
				position.width - note.margin_left - note.margin_right,
				size);

			EditorGUI.HelpBox(note_rect, note.text, message_type);
		}

		/// <inheritdoc />
		public override float GetHeight()
		{
			var note = attribute as NoteAttribute;
			var content = new GUIContent(note.text);
			var style = EditorStyles.helpBox;
			float size;
			try
			{
				// When compiling, the GetHeight can be called outside the OnGUI path
				// Unity has not exposed a check that does not throw an exception if this is the case
				// That is why we need to try and catch here.
				var current_width = EditorGUIUtility.currentViewWidth;
				size = style.CalcHeight(content, current_width - note.margin_left - note.margin_right);
			}
			catch
			{
				size = 24;
			}

			if (size < note.height_min)
				size = note.height_min;

			return size + note.margin_top + note.margin_bottom;
		}

		/// <remarks>
		///		Manual conversion in case Unity changes their editor MessageType values.
		/// </remarks>
		public static MessageType IntToMessageType(int value)
		{
			switch (value)
			{
				case NoteAttribute.None: return MessageType.None;
				case NoteAttribute.Info: return MessageType.Info;
				case NoteAttribute.Warning: return MessageType.Warning;
				case NoteAttribute.Error: return MessageType.Error;
			}

			return MessageType.None;
		}
	}
}
