using System;
using UnityEngine;
using insitu;


public class test_rotation_main : MonoBehaviour
{
	[NonSerialized] public array<double4> Positions;
	[NonSerialized] public array<double4> Reference;

	public test_rotation_point[] Points;
	public Transform Result;
	public bool QueueInitial;

	void Update()
	{
		var points = Points;
		var length = points.Length;
		var positions = Positions = Positions.Reuse(length);
		for (var i = 0; i < length; i++)
			positions[i] = points[i].Position();

		var recalculate_reference = false;
		var reference = Reference.Reuse(length);
		if (reference.elements != Reference.elements)
		{
			Reference = reference;
			recalculate_reference = true;
		}

		if (recalculate_reference || QueueInitial)
		{
			for (var i = 0; i < length; i++)
				reference[i] = positions[i];
		}

		var matrix = double4x4.transformed_by(length, reference, positions);
		var position = matrix.position();
		var rotation = matrix.rotation();
		Result.localRotation = rotation.q();
		Result.localPosition = position.v3();
		return;
	}
}
