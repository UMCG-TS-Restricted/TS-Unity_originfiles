using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using ADG;
using System;


namespace insitu
{
	/// <summary>
	///		Representation of four-dimensional vectors.
	///		Useful to represent vectors and quaternions.
	/// </summary>
	/// <seealso cref="Vector4"/>
	/// <seealso cref="Quaternion"/>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct double4
	{
		/// <summary>
		///		A valid 0 rotation quaternion: [0, 0, 0, 1].
		/// </summary>
		public static double4 identity => new double4 { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };

		public double x;
		public double y;
		public double z;
		public double w;

		/// <summary>
		///		The squared value of the magnitude of the vector.
		///		This saves a sqrt call; useful for distance comparisons.
		/// </summary>
		public double sqrmagnitude() => x * x + y * y + z * z + w * w;

		/// <summary>
		///		Normalize a vector.
		///		When the vector has a zero magnitude, a zero value will be returned.
		/// </summary>
		/// <seealso cref="normalizeq"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 normalize(double4 v)
		{
			var n = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w);
			var s = n <= 0.00001 ? 0 : 1.0 / n;
			return new double4
			{
				x = s * v.x,
				y = s * v.y,
				z = s * v.z,
				w = s * v.w,
			};
		}

		/// <summary>
		///		Normalize a quaternion.
		///		When the quaternion has a zero magnitude, an identity value will be returned.
		/// </summary>
		/// <seealso cref="normalize"/>
		/// <seealso cref="identity"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 normalizeq(double4 v)
		{
			var n = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w);
			if (n <= 0.00001)
				return identity;

			var s = 1.0 / n;
			return new double4
			{
				x = s * v.x,
				y = s * v.y,
				z = s * v.z,
				w = s * v.w,
			};
		}

		/// <summary>
		///		Converts an array to a vector.
		/// </summary>
		/// <param name="v">Array containing the vector</param>
		/// <param name="index">The offset in the array to start converting.</param>
		/// <param name="fallback">When the element in the array does not exists, these values will be used</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 from(Json.Array v, int index, double4 fallback)
		{
			if (v == null)
				return fallback;

			double4 result = fallback;
			if (v.Count > index + 0) result.x = v[index + 0];
			if (v.Count > index + 1) result.y = v[index + 1];
			if (v.Count > index + 2) result.z = v[index + 2];
			if (v.Count > index + 3) result.w = v[index + 3];
			return result;
		}

		/// <summary>Convert to doubles.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 from(Vector4 v) => new double4 { x = v.x, y = v.y, z = v.z, w = v.w, };

		/// <summary>Convert to doubles.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 from(Quaternion v) => new double4 { x = v.x, y = v.y, z = v.z, w = v.w, };

		/// <summary>Convert to floats.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Quaternion q() => new Quaternion((float)x, (float)y, (float)z, (float)w);

		/// <summary>Convert to floats and remove the w-component.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 v3() => new Vector3((float)x, (float)y, (float)z);

		/// <summary>Remove the w-component and keep a double3 with the x, y and z values.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double3 d3() => new double3 { x = x, y = y, z = z };

		/// <summary>Convert the object to a Json array.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Json.Array json() => new Json.Array { x, y, z, w };

		/// <summary>
		///		Combines rotation <paramref name="a"/> and <paramref name="b"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 operator *(double4 a, Quaternion b) => new double4
		{
			x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
			y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
			z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
			w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
		};

		/// <summary>
		///		Combines rotation <paramref name="a"/> and <paramref name="b"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 rotate_quaternion(double4 a, double4 b) => new double4
		{
			x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
			y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
			z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
			w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
		};

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
		public static double4 Smooth(double4 current, double4 target, ref double4 velocity, double deltaTime, double smoothTime)
		{
			var ti = 2 / smoothTime;
			var td = ti * deltaTime;
			var ts = 1 / (1 + td + 0.48 * td * td + 0.235 * td * td * td);
			var delta_x = current.x - target.x;
			var delta_y = current.y - target.y;
			var delta_z = current.z - target.z;
			var delta_w = current.w - target.w;
			var raw_x = (velocity.x + ti * delta_x) * deltaTime;
			var raw_y = (velocity.y + ti * delta_y) * deltaTime;
			var raw_z = (velocity.z + ti * delta_z) * deltaTime;
			var raw_w = (velocity.w + ti * delta_w) * deltaTime;
			var vel_x = (velocity.x - ti * raw_x) * ts;
			var vel_y = (velocity.y - ti * raw_y) * ts;
			var vel_z = (velocity.z - ti * raw_z) * ts;
			var vel_w = (velocity.w - ti * raw_w) * ts;
			var cur_x = current.x - delta_x + (delta_x + raw_x) * ts;
			var cur_y = current.y - delta_y + (delta_y + raw_y) * ts;
			var cur_z = current.z - delta_z + (delta_z + raw_z) * ts;
			var cur_w = current.w - delta_w + (delta_w + raw_w) * ts;
			var pre_x = target.x - current.x;
			var pre_y = target.y - current.y;
			var pre_z = target.z - current.z;
			var pre_w = target.w - current.w;
			var del_x = cur_x - target.x;
			var del_y = cur_y - target.y;
			var del_z = cur_z - target.z;
			var del_w = cur_w - target.w;

			if (pre_x * del_x + pre_y * del_y + pre_z * del_z + pre_w * del_w > 0)
			{
				cur_x = target.x;
				cur_y = target.y;
				cur_z = target.z;
				cur_w = target.w;
				vel_x = (cur_x - target.x) / deltaTime;
				vel_y = (cur_y - target.y) / deltaTime;
				vel_z = (cur_z - target.z) / deltaTime;
				vel_w = (cur_w - target.w) / deltaTime;
			}

			velocity = new double4 { x = vel_x, y = vel_y, z = vel_z, w = vel_w };
			return new double4 { x = cur_x, y = cur_y, z = cur_z, w = cur_w };
		}
	}
}
