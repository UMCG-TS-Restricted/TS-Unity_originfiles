using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ADG;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Representation of three-dimensional vectors.
	///		Useful to represent vectors and positions.
	/// </summary>
	/// <seealso cref="Vector3"/>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct double3
	{
		public double x;
		public double y;
		public double z;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 from(Vector3 v) => new double3 { x = v.x, y = v.y, z = v.z };

		public static double3 from(Json.Array json)
		{
			if (json == null)
				return default;

			var result = new double3();
			if (json.Count > 0) result.x = json[0];
			if (json.Count > 1) result.y = json[1];
			if (json.Count > 2) result.z = json[2];
			return result;
		}


		/// <summary>
		///		Add a w-component to the existing vector.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double4 d4(double w) => new double4 { x = x, y = y, z = z, w = w };

		/// <summary>
		///		Convert to a Unity float Vector3.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 v3() => new Vector3((float)x, (float)y, (float)z);

		/// <summary>
		///		Convert to a Json array.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Json.Array json() => new Json.Array { x, y, z };

		/// <summary>
		///		Dot product of <paramref name="l"/> and <paramref name="r"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double dot(double3 l, double3 r) => l.x * r.x + l.y * r.y + l.z * r.z;

		/// <summary>
		///		The squared value of the magnitude of the vector.
		///		This saves a sqrt call; useful for distance comparisons.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double sqrmagnitude(double3 v) => v.x * v.x + v.y * v.y + v.z * v.z;

		/// <summary>
		///		Cross product of <paramref name="l"/> and <paramref name="r"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 cross(double3 l, double3 r) => new double3
		{
			x = l.y * r.z - l.z * r.y,
			y = l.z * r.x - l.x * r.z,
			z = l.x * r.y - l.y * r.x,
		};

		/// <summary>
		///		Normalize a vector.
		///		When the vector has a zero magnitude, a zero value will be returned.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 normalize(double3 v)
		{
			var n = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
			var s = n <= 0.00001 ? 0 : 1.0 / n;
			return new double3
			{
				x = s * v.x,
				y = s * v.y,
				z = s * v.z,
			};
		}

		/// <summary>
		///		Linearely interpolate between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>.
		/// </summary>
		/// <remarks>
		///		<paramref name="t"/> is not clamped.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 lerp(double3 a, double3 b, float t) => new double3
		{
			x = a.x + (b.x - a.x) * t,
			y = a.y + (b.y - a.y) * t,
			z = a.z + (b.z - a.z) * t,
		};

		/// <summary>
		///		An extension of lerp which adds validity checks on <paramref name="a"/> and <paramref name="b"/>.
		/// </summary>
		/// <param name="va">
		///		0: <paramref name="a"/> is invalid.
		///		otherwise: <paramref name="a"/> is valid.
		/// </param>
		/// <param name="vb">
		///		0: <paramref name="b"/> is invalid.
		///		otherwise: <paramref name="b"/> is valid.
		/// </param>
		/// <seealso cref="pose"/>
		/// <seealso cref="lerp(double3, double3, float)"/>
		public static double3 lerp(double3 a, byte va, double3 b, byte vb, float t)
		{
			if (vb == 0) t = 0;
			if (va == 0) t = 1;
			return new double3
			{
				x = a.x + (b.x - a.x) * t,
				y = a.y + (b.y - a.y) * t,
				z = a.z + (b.z - a.z) * t,
			};
		}

		/// <summary>
		///		Scale <paramref name="r"/> by -1.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator -(double3 r) => new double3 { x = -r.x, y = -r.y, z = -r.z };

		/// <summary>
		///		Subtract <paramref name="r"/> from <paramref name="l"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator - (double3 l, double3 r) => new double3 { x = l.x - r.x, y = l.y - r.y, z = l.z - r.z };

		/// <summary>
		///		Sum of <paramref name="l"/> and <paramref name="r"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator +(double3 l, double3 r) => new double3 { x = l.x + r.x, y = l.y + r.y, z = l.z + r.z };

		/// <summary>
		///		Sum of <paramref name="l"/> and <paramref name="r"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator +(double3 l, Vector3 r) => new double3 { x = l.x + r.x, y = l.y + r.y, z = l.z + r.z };

		/// <summary>
		///		Divide every component in <paramref name="l"/> by <paramref name="r"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator /(double3 l, int r) => new double3 { x = l.x / r, y = l.y / r, z = l.z / r };
		
		/// <summary>
		///		Scale <paramref name="l"/> by <paramref name="r"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator *(double3 l, double r) => new double3 { x = l.x * r, y = l.y * r, z = l.z * r };


		/// <summary>
		///		Gradually changes a value towards a desired goal over time.
		///		The value is smoothed by some spring-damper like function, which will never overshoot.
		/// </summary>
		/// <remarks>
		///		This can also be used to interpolate Quaternions, but be sure to normalize the result aftwards.
		/// </remarks>
		/// <param name="current">Current value</param>
		/// <param name="target">Value trying to reach</param>
		/// <param name="velocity">The current velocity, this value is modified by the function every time you call it.</param>
		/// <param name="deltaTime">The time since the last call to this function.</param>
		/// <param name="smoothTime">Approximately the time it will take to reach the target.</param>
		/// <seealso cref="normalizeq"/>
		public static double3 Smooth(double3 current, double3 target, ref double3 velocity, double deltaTime, double smoothTime)
		{
			var ti = 2 / smoothTime;
			var td = ti * deltaTime;
			var ts = 1 / (1 + td + 0.48 * td * td + 0.235 * td * td * td);
			var delta_x = current.x - target.x;
			var delta_y = current.y - target.y;
			var delta_z = current.z - target.z;
			var raw_x = (velocity.x + ti * delta_x) * deltaTime;
			var raw_y = (velocity.y + ti * delta_y) * deltaTime;
			var raw_z = (velocity.z + ti * delta_z) * deltaTime;
			var vel_x = (velocity.x - ti * raw_x) * ts;
			var vel_y = (velocity.y - ti * raw_y) * ts;
			var vel_z = (velocity.z - ti * raw_z) * ts;
			var cur_x = current.x - delta_x + (delta_x + raw_x) * ts;
			var cur_y = current.y - delta_y + (delta_y + raw_y) * ts;
			var cur_z = current.z - delta_z + (delta_z + raw_z) * ts;
			var pre_x = target.x - current.x;
			var pre_y = target.y - current.y;
			var pre_z = target.z - current.z;
			var del_x = cur_x - target.x;
			var del_y = cur_y - target.y;
			var del_z = cur_z - target.z;

			if (pre_x * del_x + pre_y * del_y + pre_z * del_z > 0)
			{
				cur_x = target.x;
				cur_y = target.y;
				cur_z = target.z;
				vel_x = (cur_x - target.x) / deltaTime;
				vel_y = (cur_y - target.y) / deltaTime;
				vel_z = (cur_z - target.z) / deltaTime;
			}

			velocity = new double3 { x = vel_x, y = vel_y, z = vel_z };
			return new double3 { x = cur_x, y = cur_y, z = cur_z };
		}
	}
}
