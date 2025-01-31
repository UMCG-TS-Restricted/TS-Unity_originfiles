using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Static data used in gizmos.
	///		This is required as it is unknown what renderer is used, thus what materials to create.
	///		The materials should however always be controllable by _Color or _BaseColor and  _EmissionColor.
	/// </summary>
	[CreateAssetMenu(fileName = "Gizmos", menuName = "insitu/Gizmo Data")]
	public class GizmoData : ScriptableObject
	{
		public Gizmo[] Gizmos;
		public Material[] GizmoMaterials;
		public TextComponent[] GizoTexts;

		/// <summary>
		///		Instantiates a text object by <paramref name="type"/>.
		/// </summary>
		public TextComponent Text(int type, Transform parent)
		{
			var asset = GizoTexts.SafeClampedAt(type);
			if (asset == null)
				return null;

			var instance = Instantiate(asset, parent, false);
			instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
			return instance;
		}
		/// <summary>
		///		Directly reference a material by <paramref name="type"/>.
		/// </summary>
		/// <remarks>
		///		Do not change the properties of the material as it will be changed to all renderers using the material.
		///		Instead instantiate the material first.
		/// </remarks>
		public Material Material(int type) => GizmoMaterials.SafeClampedAt(type);

		/// <summary>
		///		Instantiates a material by <paramref name="type"/> and <paramref name="color"/>.
		/// </summary>
		public Material Material(int type, Color color)
		{
			var asset = GizmoMaterials.SafeClampedAt(type);
			if (asset == null)
				return null;

			var instance = Instantiate(asset);
			instance.hideFlags = HideFlags.HideAndDontSave;
			Apply(instance, color);
			return instance;
		}

		/// <summary>
		///		Applies <paramref name="color"/> to <paramref name="material"/>.
		/// </summary>
		public static void Apply(Material material, Color color)
		{
			material.SetColor("_Color", color);
			material.SetColor("_BaseColor", color);
			material.SetColor("_EmissionColor", color * 0.1f);
		}

		/// <summary>
		///		Instantiates a new Gizmo.
		/// </summary>
		public Gizmo Create(int type, int cap_start, int cap_end, Transform parent, Material material)
		{
			var asset = Gizmos.SafeClampedAt(type);
			if (asset == null)
				return null;

			var instance = Instantiate(asset, parent, false);
			instance.gameObject.hideFlags = HideFlags.HideAndDontSave;

			var renderers = instance.Renderers;
			for (var i = 0; i < renderers.Length; i++)
			{
				var renderer = renderers[i];
				renderer.sharedMaterial = material;
			}

			var caps_start = instance.CapsStart;
			for (var i = 0; i < caps_start.Length; i++)
			{
				var cap = caps_start[i];
				cap.SetActive(i == cap_start);
			}

			var caps_end = instance.CapsEnd;
			for (var i = 0; i < caps_end.Length; i++)
			{
				var cap = caps_end[i];
				cap.SetActive(i == cap_end);
			}

			return instance;
		}
	}
}
