using System.Collections.Generic;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Miscellaneous procedures
	/// </summary>
	public static class Util
	{
		/// <summary>
		///		Wrapper around Object.Destroy with null checking.
		/// </summary>
		/// <seealso cref="Object.Destroy(Object)"/>
		public static void Destroy<T>(ref T obj) where T : Object
		{
			if (obj)
			{
				Object.Destroy(obj);
				obj = null;
			}
		}

		/// <summary>
		///		Wrapper around Object.Destroy(obj.gameObject) with null checking.
		/// </summary>
		/// <seealso cref="Object.Destroy(Object)"/>
		public static void DestroyEntity<T>(ref T obj) where T : Component
		{
			if (obj)
			{
				Object.Destroy(obj.gameObject);
				obj = null;
			}
		}

		/// <summary>
		///		Destroy all gameObjects of components inside <paramref name="items"/>.
		/// </summary>
		/// <remarks>
		///		<paramref name="items"/> must not be of value null.
		/// </remarks>
		public static void Clear<T>(List<T> items) where T : Component
		{
			for (var i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (item)
					Object.Destroy(item.gameObject);
			}

			items.Clear();
		}

		/// <summary>
		///		Destroy all children in <paramref name="transform"/>.
		/// </summary>
		/// <remarks>
		///		<paramref name="transform"/> must not be of value null.
		/// </remarks>
		public static void Clear(Transform transform)
		{
			var count = transform.childCount;
			for (var i = 0; i < count; i++)
			{
				var child = transform.GetChild(i);
				Object.Destroy(child.gameObject);
			}
		}

		/// <summary>
		///		Clamps <paramref name="index"/> withing the range of <paramref name="list"/>.
		///		If the <paramref name="list"/> is null or has a length of 0; a default <typeparamref name="T"/> value is returned.
		/// </summary>
		public static T SafeClampedAt<T>(this IList<T> list, int index)
		{
			if (list == null || list.Count == 0)
				return default;

			if (index < 0)
				return list[0];

			var length = list.Count;
			if (index >= length)
				return list[length - 1];

			return list[index];
		}

		/// <summary>
		///		Clamps <paramref name="index"/> withing the range of <paramref name="list"/>.
		/// </summary>
		/// <remarks>
		///		<paramref name="list"/> must not be of value null.
		///		The procedure does not check null references.
		///		If null checking is preferable, use SafeClampedAt instead.
		///	</remarks>
		/// <seealso cref="SafeClampedAt"/>
		public static T ClampedAt<T>(this IList<T> list, int index)
		{
			if (index < 0)
				return list[0];

			var length = list.Count;
			if (index >= length)
				return list[length - 1];

			return list[index];
		}
	}
}
