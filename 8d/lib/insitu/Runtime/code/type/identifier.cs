using System;
using UnityEngine;


namespace insitu
{
	[Serializable]
	public struct identifier
	{
		public static readonly string PrefKey = "insitu_idx";

		public int value;

		public identifier ensure()
		{
			if (value == 0)
				value = request();

			return this;
		}

		public static identifier create() => new identifier { value = request() };
		public static int request()
		{
			unchecked
			{
				#if UNITY_EDITOR
				var value = 1 + UnityEditor.EditorPrefs.GetInt(PrefKey, 0);
				UnityEditor.EditorPrefs.SetInt(PrefKey, value);
				PlayerPrefs.SetInt(PrefKey, value);
				PlayerPrefs.Save();
				return value;
				#else
				var value = 1 + PlayerPrefs.GetInt(PrefKey, 0);
				PlayerPrefs.SetInt(PrefKey, value);
				PlayerPrefs.Save();
				return value;
				#endif
			}
		}

		public static implicit operator int(identifier id) => id.value;
	}
}
