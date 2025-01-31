using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Gizmo collection to represent an arc.
	/// </summary>
	public class GizmoAngle : GizmoBase
	{
		public const float RadiusArc = 0.2f;
		public const float RadiusText = 0.3f;
		public const float Thickness = 3f;

		[NonSerialized] public ControlPoint[] Arc;
		[NonSerialized] public Vector3 Normal;
		[NonSerialized] public Gizmo GizmoCa;
		[NonSerialized] public Gizmo GizmoCb;
		[NonSerialized] public Gizmo GizmoArc;
		[NonSerialized] public Material Material;
		[NonSerialized] public TextComponent Text;

		public PoseBehaviour PoseStart;
		public PoseBehaviour PoseCenter;
		public PoseBehaviour PoseEnd;


		public void OnDestroy()
		{
			Util.DestroyEntity(ref GizmoCa);
			Util.DestroyEntity(ref GizmoCb);
			Util.DestroyEntity(ref GizmoArc);
			Util.Destroy(ref Material);
			Util.DestroyEntity(ref Text);
		}

		public void OnDisable()
		{
			if (GizmoCa) GizmoCa.gameObject.SetActive(false);
			if (GizmoCb) GizmoCb.gameObject.SetActive(false);
			if (GizmoArc) GizmoArc.gameObject.SetActive(false);
			if (Text) Text.gameObject.SetActive(false);
		}

		public int Evaluate(
			out Vector3 position_start,
			out Vector3 position_center,
			out Vector3 position_end,
			out Vector3 position_text,
			out Vector3 normal,
			out float angle)
		{
			position_start = default;
			position_center = default;
			position_end = default;
			position_text = default;
			normal = default;
			angle = default;

			if (!PoseStart || !PoseCenter || !PoseEnd)
				return -1;

			var pose_start = PoseStart.Pose();
			var pose_center = PoseCenter.Pose();
			var pose_end = PoseEnd.Pose();
			if (pose_start.valid_position == 0 ||
				pose_center.valid_position == 0 ||
				pose_end.valid_position == 0)
				return -1;

			position_start = pose_start.position.v3();
			position_center = pose_center.position.v3();
			position_end = pose_end.position.v3();

			var arc = Arc;
			if (arc == null)
				Arc = arc = new ControlPoint[8];

			Vector3 axis;
			var p01 = (position_start - position_center).normalized;
			var p21 = (position_end - position_center).normalized;
			normal = Vector3.Cross(p01, p21);
			var dot = Vector3.Dot(p01, p21);
			if (Normal.sqrMagnitude > 0.9f)
				axis = Normal;
			else if (dot > 0.1f && dot < -0.9f)
				Normal = axis = normal;
			else axis = normal;

			angle = Mathf.Acos(dot);
			if (Vector3.Dot(axis, normal) < 0)
			{
				angle = 2 * Mathf.PI - angle;
				normal = -normal;
			}

			const float threshold = 3.0f / Mathf.PI;
			var segments = (int)(0.99999f + angle * threshold);
			if (segments >= arc.Length)
				segments = arc.Length - 1;

			var segment_angle = angle / segments;
			var segment_deg = segment_angle * Mathf.Rad2Deg;
			var segment_mul = Mathf.Abs(segment_angle) / Mathf.PI;
			for (var i = 0; i <= segments; i++)
			{
				var deg = i * segment_deg;
				var v0 = Quaternion.AngleAxis(deg, normal) * p01;
				var v1 = Quaternion.AngleAxis(deg + 90, normal) * p01;
				arc[i] = new ControlPoint
				{
					position = position_center + v0 * RadiusArc,
					vector = v1 * RadiusArc * segment_mul,
				};
			}

			position_text = position_center;
			position_text += Quaternion.AngleAxis(angle / 2 * Mathf.Rad2Deg, normal) * (p01 * RadiusText);
			return segments;
		}

		public override void UpdateEditorGizmo(float alpha)
		{
			if (alpha <= 0)
				return;

			var count = Evaluate(
				out var position_start,
				out var position_center,
				out var position_end,
				out var position_text,
				out var normal,
				out var angle);
			if (count < 0)
				return;

			#if UNITY_EDITOR
			var p01 = (position_start - position_center).normalized;
			UnityEditor.Handles.color = new Color(0.49f, 0.004f, 0.941f, alpha);
			UnityEditor.Handles.DrawLine(position_center, position_start, Thickness);
			UnityEditor.Handles.DrawLine(position_center, position_end, Thickness);
			UnityEditor.Handles.DrawSolidArc(position_center, normal, p01, angle * Mathf.Rad2Deg, RadiusArc);
			UnityEditor.Handles.Label(position_text, $"{angle * Mathf.Rad2Deg:0.0}°");
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
				out var position_center,
				out var position_end,
				out var position_text,
				out var normal,
				out var angle);
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

			var gizmo_ca = GizmoCa;
			if (!gizmo_ca) GizmoCa = gizmo_ca = Data.Create(0, -1, 0, transform, material);
			var gizmo_cb = GizmoCb;
			if (!gizmo_cb) GizmoCb = gizmo_cb = Data.Create(0, -1, 0, transform, material);
			var gizmo_arc = GizmoArc;
			if (!gizmo_arc) GizmoArc = gizmo_arc = Data.Create(1, -1, -1, transform, material);
			if (!gizmo_ca || !gizmo_cb || !gizmo_arc)
			{
				OnDisable();
				return;
			}

			gizmo_ca.Evaluate(position_center, position_start, normal);
			gizmo_cb.Evaluate(position_center, position_end, normal);
			gizmo_arc.Cache = Arc;
			gizmo_arc.Evaluate(count, normal);

			gizmo_ca.gameObject.SetActive(true);
			gizmo_cb.gameObject.SetActive(true);
			gizmo_arc.gameObject.SetActive(true);

			var text = Text;
			if (!text) Text = text = Data.Text(0, transform);
			if (text)
			{
				text.text = $"{angle * Mathf.Rad2Deg:0.0}°";
				text.color = new Color(0.8f, 0.5f, 1, alpha);
				text.transform.position = position_text;
				text.gameObject.SetActive(true);
			}
		}
	}
}
