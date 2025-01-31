using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace insitu
{
	public static class EditorUtil
	{
		public static void FindAll<T>(List<T> list) where T : Object
		{
			list.Clear();
			var guids = AssetDatabase.FindAssets("t:"+typeof(T));
			for (var i = 0; i < guids.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				var asset = AssetDatabase.LoadAssetAtPath<T>(path);
				if (asset)
					list.Add(asset);
			}
		}

		public static void FindAll<T>(List<T> list, string search_pattern) where T : Object
		{
			list.Clear();
			var guids = AssetDatabase.FindAssets(search_pattern);
			for (var i = 0; i < guids.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				var asset = AssetDatabase.LoadAssetAtPath<T>(path);
				if (asset)
					list.Add(asset);
			}
		}
	}
}
