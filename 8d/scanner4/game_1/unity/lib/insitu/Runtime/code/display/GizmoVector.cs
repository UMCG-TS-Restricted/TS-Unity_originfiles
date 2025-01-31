using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Gizmo to represent a vector.
	/// </summary>
	public class GizmoVector : GizmoBase
	{
		public const float Thickness = 3f;

		[NonSerialized] public Gizmo Gizmo;
		[NonSerialized] public Material Material;

		public PoseBehaviour PoseStart;
		public UnityStateDevice Device;
		public string AxisX;
		public string AxisY;
		public string AxisZ;
		public float Scale = 1;


		public void OnDestroy()
		{
			Util.Destroy(ref Gizmo);
			Util.Destroy(ref Material);
		}

		public void OnDisable()
		{
			if (Gizmo) Gizmo.gameObject.SetActive(false);
		}

		public int Evaluate(
			out Vector3 position_start,
			out Vector3 position_end)
		{
			position_start = default;
			position_end = default;

			if (!Device)
				return -1;

			var device = Device;
			var output_x = device.OutputOf(AxisX);
			var output_y = device.OutputOf(AxisY);
			var output_z = device.OutputOf(AxisZ);
			if (output_x.valid == 0 ||
				output_y.valid == 0 ||
				output_z.valid == 0)
				return -1;

			if (PoseStart)
			{
				var pose_start = PoseStart.Pose();
				if (pose_start.valid_position == 0)
					return -1;

				position_start = pose_start.position.v3();
			}
			else
			{
				position_start = transform.position;
			}

			position_end = position_start + Scale * new Vector3(
				(float)output_x.value,
				(float)output_y.value,
				(float)output_z.value);

			return 2;
		}

		public override void UpdateEditorGizmo(float alpha)
		{
			if (alpha <= 0)
				return;

			var count = Evaluate(
				out var position_start,
				out var position_end);
			if (count < 0)
				return;

			#if UNITY_EDITOR
			UnityEditor.Handles.color = new Color(0.49f, 0.004f, 0.941f, alpha);
			UnityEditor.Handles.DrawLine(position_start, position_end, Thickness);
			#endif
		}

		public override void UpdatePlayerGizmo(float alpha)
		{
			if (alpha <= 0)
			{
				OnDisable();
				return;
			}

			var count = Evaluate(
				out var position_start,
				out var position_end);
			if (count < 0)
			{
				OnDisable();
				return;
			}

			var material = Material;
			if (!material)
			{
				Material = material = Data.Material(0, Color.white);
				if (!material)
				{
					OnDisable();
					return;
				}
			}

			GizmoData.Apply(material, new Color(0.49f, 0.004f, 0.941f, alpha));

			var gizmo = Gizmo;
			if (!gizmo)
				Gizmo = gizmo = Data.Create(0, -1, 0, transform, material);
			if (!gizmo)
			{
				OnDisable();
				return;
			}

			gizmo.Evaluate(position_start, position_end, Vector3.up);
		}
	}
}
