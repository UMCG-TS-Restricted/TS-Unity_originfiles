using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		A spline with caps.
	/// </summary>
	public class Gizmo : MonoBehaviour
	{
		[NonSerialized] public ControlPoint[] Cache;
		[NonSerialized] public array<Vector3> Positions;
		[NonSerialized] public array<Quaternion> Rotations;

		public Transform RotationOffset;
		public Renderer[] Renderers;
		public Transform[] Bones;
		public GameObject[] CapsStart;
		public GameObject[] CapsEnd;

		public ControlPoint[] RequestCache => Cache == null ? (Cache = new ControlPoint[16]) : Cache;

		/// <summary>
		///		Initializes renderes to use <paramref name="material"/>.
		/// </summary>
		public void Initialize(Material material)
		{
			var renderers = Renderers;
			for (var i = 0; i < renderers.Length; i++)
			{
				var ren = renderers[i];
				ren.material = material;
			}
		}

		/// <summary>
		///		Update spline by values in Cache.
		/// </summary>
		/// <param name="length">Amount of control points used</param>
		/// <param name="reference">Up/normal vector</param>
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
				bones[i].SetPositionAndRotation(positions[i], rotations[i]);
		}

		/// <summary>
		///		Transforms the spline into a line between <paramref name="c0"/> and <paramref name="c1"/>.
		/// </summary>
		/// <param name="c0">start position</param>
		/// <param name="c1">end position</param>
		/// <param name="reference">Up/normal vector</param>
		public void Evaluate(Vector3 c0, Vector3 c1, Vector3 reference)
		{
			var bones = Bones;
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
				bones[i].SetPositionAndRotation(positions[i], rotations[i]);
		}

		/// <summary>
		///		Transforms the spline into an arc.
		/// </summary>
		/// <param name="c0">Start point</param>
		/// <param name="c1">Center point</param>
		/// <param name="c2">End point</param>
		/// <param name="reference">Up/normal vector</param>
		/// <param name="radius">Arc radius</param>
		/// <returns>Angle in radians between the vectors c0-c1 and c2-c1</returns>
		public float Evaluate(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 reference, float radius)
		{
			var cache = RequestCache;
			var length = ControlPoint.arc(c0, c1, c2, reference, radius, out var angle, cache);
			Evaluate(length, reference);
			return angle;
		}
	}
}
