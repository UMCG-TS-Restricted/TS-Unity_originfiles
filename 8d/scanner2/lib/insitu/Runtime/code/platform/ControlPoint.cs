using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Control points used for spline calculations
	/// </summary>
	public struct ControlPoint
	{
		public Vector3 position;
		public Vector3 vector;

		/// <summary>
		///		Interpolate using Hermite spline function.
		/// </summary>
		public static Vector3 evaluate(ControlPoint start, ControlPoint end, float t)
		{
			var c0 = start.position + start.vector;
			var c1 = end.position - end.vector;
			var tt = t * t;
			var ttt = tt * t;
			var u = 1.0f - t;
			var uu = u * u;
			var uuu = uu * u;
			var utt3 = 3 * u * tt;
			var uut3 = 3 * uu * t;
			return new Vector3(
				uuu * start.position.x + uut3 * c0.x + utt3 * c1.x + ttt * end.position.x,
				uuu * start.position.y + uut3 * c0.y + utt3 * c1.y + ttt * end.position.y,
				uuu * start.position.z + uut3 * c0.z + utt3 * c1.z + ttt * end.position.z);
		}

		/// <summary>
		///		Determine the angle and control points of an arc.
		///		Use this if you want full 0.. 360 degree angle calculations.
		/// </summary>
		/// <param name="p0">Start position</param>
		/// <param name="p1">Central position</param>
		/// <param name="p2">End position</param>
		/// <param name="axis">Reference axis plane on which the points are located to allow for 360 degree calculations</param>
		/// <param name="radius">Distance from p1 on which to calculate the control points</param>
		/// <param name="angle">Angle in radians between the vectors p0-p1 and p2-p1</param>
		/// <param name="result">Cache to which to write the control points to</param>
		/// <returns>The last index of the written control point in result</returns>
		public static int arc(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 axis, float radius, out float angle, ControlPoint[] result)
		{
			var p01 = (p0 - p1).normalized;
			var p21 = (p2 - p1).normalized;
			var normal = Vector3.Cross(p01, p21);
			var dot = Vector3.Dot(p01, p21);
			angle = Mathf.Acos(dot);
			if (Vector3.Dot(axis, normal) < 0)
			{
				angle = 2 * Mathf.PI - angle;
				normal = -normal;
			}

			const float threshold = 3.0f / Mathf.PI;
			var segments = (int)(0.99999f + angle * threshold);
			if (segments >= result.Length)
				segments = result.Length - 1;

			var segment_angle = angle / segments;
			var segment_deg = segment_angle * Mathf.Rad2Deg;
			var segment_mul = Mathf.Abs(segment_angle) / Mathf.PI;
			for (var i = 0; i <= segments; i++)
			{
				var deg = i * segment_deg;
				var v0 =  Quaternion.AngleAxis(deg, normal) * p01;
				var v1 = Quaternion.AngleAxis(deg + 90, normal) * p01;
				result[i] = new ControlPoint
				{
					position = p1 + v0 * radius,
					vector = v1 * radius * segment_mul,
				};
			}

			return segments;
		}

		/// <summary>
		///		Determine the angle and control points of an arc.
		///		Use this if you want the shortest angle between the vectors; 0.. 180 degree angles.
		/// </summary>
		/// <param name="p0">Start position</param>
		/// <param name="p1">Central position</param>
		/// <param name="p2">End position</param>
		/// <param name="radius">Distance from p1 on which to calculate the control points</param>
		/// <param name="angle">Angle in radians between the vectors p0-p1 and p2-p1</param>
		/// <param name="result">Cache to which to write the control points to</param>
		/// <returns>The last index of the written control point in result</returns>
		public static int arc(Vector3 p0, Vector3 p1, Vector3 p2, float radius, out float angle, ControlPoint[] result)
		{
			var p01 = (p0 - p1).normalized;
			var p21 = (p2 - p1).normalized;
			var normal = Vector3.Cross(p01, p21);
			var dot = Vector3.Dot(p01, p21);
			angle = Mathf.Acos(dot);

			const float threshold = 3.0f / Mathf.PI;
			var segments = (int)(0.99999f + angle * threshold);
			if (segments >= result.Length)
				segments = result.Length - 1;

			var segment_angle = angle / segments;
			var segment_deg = segment_angle * Mathf.Rad2Deg;
			var segment_mul = Mathf.Abs(segment_angle) / Mathf.PI;
			for (var i = 0; i <= segments; i++)
			{
				var deg = i * segment_deg;
				var v0 =  Quaternion.AngleAxis(deg, normal) * p01;
				var v1 = Quaternion.AngleAxis(deg + 90, normal) * p01;
				result[i] = new ControlPoint
				{
					position = p1 + v0 * radius,
					vector = v1 * radius * segment_mul,
				};
			}

			return segments;
		}

		public static void DrawGizmo(ControlPoint start, ControlPoint end)
		{
			const float radius = 0.005f;
			var color = Gizmos.color;
			Gizmos.color = new Color(color.r, color.g, color.b, color.a * 0.2f);
			Gizmos.DrawSphere(start.position, radius);
			Gizmos.DrawSphere(start.position + start.vector, radius);
			Gizmos.DrawSphere(end.position, radius);
			Gizmos.DrawSphere(end.position - end.vector, radius);

			Gizmos.color = new Color(color.r, color.g, color.b, color.a * 1);
			const int count = 10;
			const float countinv = 1.0f / count;
			for (var i = 0; i < count; i++)
			{
				var p0 = evaluate(start, end, countinv * i);
				var p1 = evaluate(start, end, countinv * (i + 1));
				Gizmos.DrawLine(p0, p1);
			}
		}
	}
}
