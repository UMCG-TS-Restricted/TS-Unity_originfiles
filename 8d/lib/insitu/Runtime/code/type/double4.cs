using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ADG;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Representation of four-dimensional vectors.
	///		Useful to represent vectors and quaternions.
	/// </summary>
	/// <seealso cref="Vector4"/>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct double4
	{
		public double x;
		public double y;
		public double z;
		public double w;

		public double sqr_magnitude() => x * x + y * y + z * z + w * w;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 from(Vector4 v) => new double4 { x = v.x, y = v.y, z = v.z, w = v.w, };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 from(Quaternion v) => new double4 { x = v.x, y = v.y, z = v.z, w = v.w, };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Quaternion q() => new Quaternion((float)x, (float)y, (float)z, (float)w);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 v3() => new Vector3((float)x, (float)y, (float)z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double3 d3() => new double3 { x = x, y = y, z = z };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Json.Array ToJson() => new Json.Array { x, y, z, w };
	}
}
