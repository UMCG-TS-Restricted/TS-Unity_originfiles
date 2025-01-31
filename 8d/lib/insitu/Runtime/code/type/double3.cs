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
	/// <seealso cref="UnityEngine.Vector3"/>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct double3
	{
		public double x;
		public double y;
		public double z;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 from(Vector3 v) => new double3 { x = v.x, y = v.y, z = v.z };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double4 d4(double w) => new double4 { x = x, y = y, z = z, w = w };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 v3() => new Vector3((float)x, (float)y, (float)z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Json.Array ToJson() => new Json.Array { x, y, z };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double dot(double3 l, double3 r) => l.x * r.x + l.y * r.y + l.z * r.z;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double sqrmagnitude(double3 v) => v.x * v.x + v.y * v.y + v.z * v.z;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 normalize(double3 v)
		{
			var n = 1.0 / Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
			return new double3
			{
				x = n * v.x,
				y = n * v.y,
				z = n * v.z,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator - (double3 l, double3 r) => new double3 { x = l.x - r.x, y = l.y - r.y, z = l.z - r.z };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator +(double3 l, double3 r) => new double3 { x = l.x + r.x, y = l.y + r.y, z = l.z + r.z };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator /(double3 l, int r) => new double3 { x = l.x / r, y = l.y / r, z = l.z / r };
	}
}
