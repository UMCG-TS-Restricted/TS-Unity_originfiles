using insitu;
using UnityEngine;
using UnityEngine.UIElements;


public class Gizmo : MonoBehaviour
{
	public ControlPoint[] Cache;
	public array<Vector3> Positions;
	public array<Quaternion> Rotations;
	public float Alpha;


	public Transform RotationOffset;
	public Renderer[] Renderers;
	public Transform[] Bones;
	public GameObject[] CapsStart;
	public GameObject[] CapsEnd;

	public ControlPoint[] RequestCache => Cache == null ? (Cache = new ControlPoint[16]) : Cache;

	public void Initialize(Material material)
	{
		var renderers = Renderers;
		for (var i = 0; i < renderers.Length; i++)
		{
			var ren = renderers[i];
			ren.material = material;
		}
	}

	public void Update()
	{
		
	}

	public void Evaluate(int length, Vector3 reference)
	{
		var bones = Bones;
		var cache = Cache;
		var capacity = bones.Length;
		var last = capacity - 1;
		var inv_last = 1.0f / last;
		var inv_length = 1.0f / length;
		var positions = Positions.Reuse(capacity);
		var average = Vector3.zero;
		for (var i = 0; i < capacity; i++)
		{
			var t = i * inv_last;
			var i0 = (int)(t * length);
			var i1 = i0 + 1;
			var ia = (t - i0 * inv_length) * length;
			var c0 = cache[i0];
			var c1 = cache[i1];
			var position = ControlPoint.evaluate(c0, c1, ia);
			positions[i] = position;
			average += position;
		}
		Positions = positions;
		average /= capacity;

		var rotation = RotationOffset.localRotation;
		var up = rotation * Vector3.up;
		var rotations = Rotations.Reuse(capacity);
		rotations[0] = Quaternion.LookRotation((positions[2] - positions[0]).normalized, reference);
		rotations[last] = Quaternion.LookRotation((positions[last] - positions[last - 1]).normalized, reference);
		for (var i = 1; i < last; i++)
		{
			var prev = positions[i - 0];
			var next = positions[i + 1];
			var vec02 = next - prev;
			var rot02 = Quaternion.LookRotation(vec02.normalized, reference);
			rotations[i] = rot02;
		}
		for (var i = 0; i < capacity; i++)
			rotations[i] = rotations[i] * rotation;

		Rotations = rotations;


		transform.position = average;
		for (var i = 0; i < capacity; i++)
		{
			bones[i].SetPositionAndRotation(positions[i], rotations[i]);
			//Debug.DrawRay(positions[i], rotations[i] * Vector3.forward);
		}
	}

	public void Evaluate(Vector3 c0, Vector3 c1, Vector3 reference)
	{
		var bones = Bones;
		var cache = Cache;
		var capacity = bones.Length;
		var last = capacity - 1;
		var inv_last = 1.0f / last;
		var positions = Positions.Reuse(capacity);
		var average = Vector3.zero;
		for (var i = 0; i < capacity; i++)
		{
			var t = i * inv_last;
			var position = Vector3.LerpUnclamped(c0, c1, t);
			positions[i] = position;
			average += position;
		}
		Positions = positions;
		average /= capacity;

		var rotation = RotationOffset.localRotation;
		var up = rotation * Vector3.up;
		var rotations = Rotations.Reuse(capacity);
		rotations[0] = Quaternion.LookRotation((positions[2] - positions[0]).normalized, reference);
		rotations[last] = Quaternion.LookRotation((positions[last] - positions[last - 1]).normalized, reference);
		for (var i = 1; i < last; i++)
		{
			var prev = positions[i - 0];
			var next = positions[i + 1];
			var vec02 = next - prev;
			var rot02 = Quaternion.LookRotation(vec02.normalized, reference);
			rotations[i] = rot02;
		}
		for (var i = 0; i < capacity; i++)
			rotations[i] = rotations[i] * rotation;

		Rotations = rotations;


		transform.position = average;
		for (var i = 0; i < capacity; i++)
		{
			bones[i].SetPositionAndRotation(positions[i], rotations[i]);
			//Debug.DrawRay(positions[i], rotations[i] * Vector3.forward);
		}
	}

	public float Evaluate(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 reference, float radius)
	{
		var cache = RequestCache;
		var length = ControlPoint.arc(c0, c1, c2, reference, radius, out var angle, cache);
		Evaluate(length, reference);
		return angle;
	}
}
