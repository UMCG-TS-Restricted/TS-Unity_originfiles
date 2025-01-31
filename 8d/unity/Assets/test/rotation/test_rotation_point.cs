using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using insitu;

public class test_rotation_point : MonoBehaviour
{
	[NonSerialized] public uint Rand;

	public bool Valid;
	public float ValidChance;
	public float NoiseAmplitude;

	public void Start()
	{
		Rand = Hash.Simple((uint)GetHashCode(), 9395453U);
	}

	public void Update()
	{
		Rand++;
	}

	public double4 Position()
	{
		var offset = new Vector3(
			Hash.Noise(Rand, 5936831U) - 0.5f,
			Hash.Noise(Rand, 2729359U) - 0.5f,
			Hash.Noise(Rand, 2695711U) - 0.5f);
		offset *= 2 * NoiseAmplitude;

		var valid = Valid && Hash.Noise(Rand, 9284207U) < ValidChance;


		var position = transform.position;
		var result = new double4();
		result.x = position.x + offset.x;
		result.y = position.y + offset.y;
		result.z = position.z + offset.z;
		result.w = valid ? 1.0f : 0.0f;
		return result;
	}

	public void OnDrawGizmos()
	{
		if (!this || !transform)
			return;

		var position = Position();
		var color = position.w < 1
			? new Color(1.0f, 0.6f, 0.2f, 0.2f)
			: new Color(1.0f, 0.4f, 0.1f, 0.8f);

		Gizmos.color = color;
		Gizmos.DrawSphere(position.v3(), 0.08f);
	}
}
