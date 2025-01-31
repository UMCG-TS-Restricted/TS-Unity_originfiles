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
	[NonSerialized] public Vector3[] Axis;
	[NonSerialized] public float[] Speed;

	public Material MaterialAsset;
	public MeshRenderer Renderer;
	public Transform[] Rings;


	public void Awake()
	{
		Axis = new Vector3[Rings.Length];
		for (var i = 0; i < Axis.Length; i++)
			Axis[i] = UnityEngine.Random.onUnitSphere;
		Speed = new float[Rings.Length];
		for (var i = 0; i < Speed.Length; i++)
			Speed[i] = UnityEngine.Random.Range(8.0f, 12.0f);
	}

	public void Apply(float time)
	{
		if (Speed == null || Axis == null)
			Awake();

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

		for (var i = 0; i < Rings.Length; i++)
		{
			var r = Rings[i];
			var t = time * Speed[i];
			r.localRotation = Quaternion.AngleAxis(t, Axis[i]);
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

