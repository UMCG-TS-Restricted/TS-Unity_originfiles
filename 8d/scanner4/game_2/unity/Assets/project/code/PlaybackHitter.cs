using System;
using UnityEngine;

public class PlaybackHitter : MonoBehaviour
{
	[NonSerialized] public bool Active;
	[NonSerialized] public int Id;
	[NonSerialized] public Material Material;
	[NonSerialized] public Color Color;
	[NonSerialized] public float Radius;
	[NonSerialized] public float Alpha;
	[NonSerialized] public Vector3 Position;

	public Material MaterialAsset;
	public MeshRenderer Renderer;

	public void Apply(float time)
	{
		if (!Active)
		{
			gameObject.SetActive(false);
			return;
		}

		gameObject.SetActive(true);
		if (!Material)
		{
			var material = Instantiate(MaterialAsset);
			Renderer.sharedMaterial = material;
			Material = material;
		}

		var color = Color;
		color.a *= Alpha;
		Material.SetColor("_BaseColor", color);
		Material.SetColor("_Color", color);
		Material.SetColor("_EmissionColor", color * 2);

		transform.localPosition = Position;

		var radius = Radius;
		transform.localScale = new Vector3(radius, radius, radius);
	}
}
