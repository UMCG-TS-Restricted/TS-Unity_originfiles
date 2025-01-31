using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Draw an info box above the selected field.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public sealed class NoteAttribute : PropertyAttribute
	{
		public const int None = 0;
		public const int Info = 1;
		public const int Warning = 2;
		public const int Error = 3;

		public readonly string text;

		public int style = Info;
		public float margin_top = 2;
		public float margin_bottom = 2;
		public float margin_left = 0;
		public float margin_right = 0;
		public float height_min = 38;

		public NoteAttribute(string text)
		{
			this.text = text;
		}
	}
}
