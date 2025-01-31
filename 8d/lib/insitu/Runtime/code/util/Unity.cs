using UnityEngine;


namespace insitu
{
	public static class Unity
	{
		public static T FindResource<T>() where T : Object
		{
			var resources = Resources.FindObjectsOfTypeAll<T>();
			for (var i = 0; i < resources.Length; i++)
			{
				var resource = resources[i];
				if (resource is T result)
					return result;
			}

			return null;
		}

		public static Color32 ToColor32(uint c) => new Color32(
			(byte)((c >> 24) & 0xFF),
			(byte)((c >> 16) & 0xFF),
			(byte)((c >>  8) & 0xFF),
			(byte)((c >>  0) & 0xFF));

		public static float SqrMagnitude(Quaternion q) => q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
	}
}
