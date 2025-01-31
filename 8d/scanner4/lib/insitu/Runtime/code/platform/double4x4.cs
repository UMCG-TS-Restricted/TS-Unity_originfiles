using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using ADG;


namespace insitu
{
	/// <summary>
	///		A standard 4x4 matrix.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct double4x4
	{
		public double m00;
		public double m01;
		public double m02;
		public double m03;

		public double m10;
		public double m11;
		public double m12;
		public double m13;

		public double m20;
		public double m21;
		public double m22;
		public double m23;

		public double m30;
		public double m31;
		public double m32;
		public double m33;

		public static readonly double4x4 identity = new double4x4
		{
			m00 = 1, m01 = 0, m02 = 0, m03 = 0,
			m10 = 0, m11 = 1, m12 = 0, m13 = 0,
			m20 = 0, m21 = 0, m22 = 1, m23 = 0,
			m30 = 0, m31 = 0, m32 = 0, m33 = 1,
		};

		/// <summary>
		///		Combines a rotation matrix with a translation vector.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4x4 from(double3x3 m, double3 p) => new double4x4
		{
			m00 = m.m00, m01 = m.m01, m02 = m.m02, m03 = p.x,
			m10 = m.m10, m11 = m.m11, m12 = m.m12, m13 = p.y,
			m20 = m.m20, m21 = m.m21, m22 = m.m22, m23 = p.z,
			m30 = 0, m31 = 0, m32 = 0, m33 = 1,
		};
		
		/// <summary>
		///		Tries to convert a Json array to a matrix.
		/// </summary>
		/// <returns>True if successful</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool from(Json.Array v, out double4x4 result)
		{
			if (v == null || v.Count != 16)
			{
				result = identity;
				return false;
			}

			result.m00 = v[0];
			result.m01 = v[1];
			result.m02 = v[2];
			result.m03 = v[3];

			result.m10 = v[4];
			result.m11 = v[5];
			result.m12 = v[6];
			result.m13 = v[7];

			result.m20 = v[8];
			result.m21 = v[9];
			result.m22 = v[10];
			result.m23 = v[11];

			result.m30 = v[12];
			result.m31 = v[13];
			result.m32 = v[14];
			result.m33 = v[15];
			return true;
		}
		
		/// <summary>
		///		Convert to a Unity matrix consisting of floats instead of doubles.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4x4 m4x4(double4x4 m) => new Matrix4x4
		{
			m00 = (float)m.m00, m01 = (float)m.m01, m02 = (float)m.m02, m03 = (float)m.m03,
			m10 = (float)m.m10, m11 = (float)m.m11, m12 = (float)m.m12, m13 = (float)m.m13,
			m20 = (float)m.m20, m21 = (float)m.m21, m22 = (float)m.m22, m23 = (float)m.m23,
			m30 = (float)m.m30, m31 = (float)m.m31, m32 = (float)m.m32, m33 = (float)m.m33,
		};

		/// <summary>
		///		Multiplies a vector with a matrix.
		///		Used to transform a point or vector from one space to another.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 mul(double4x4 l, double4 r) => new double4
		{
			x = l.m00 * r.x + l.m01 * r.y + l.m02 * r.z + l.m03 * r.w,
			y = l.m10 * r.x + l.m11 * r.y + l.m12 * r.z + l.m13 * r.w,
			z = l.m20 * r.x + l.m21 * r.y + l.m22 * r.z + l.m23 * r.w,
			w = l.m30 * r.x + l.m31 * r.y + l.m32 * r.z + l.m33 * r.w,
		};

		/// <summary>
		///		Multiplies a vector with a matrix.
		///		Used to transform a point or vector from one space to another.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 mul(double4x4 l, double3 r, double w) => new double3
		{
			x = l.m00 * r.x + l.m01 * r.y + l.m02 * r.z + l.m03 * w,
			y = l.m10 * r.x + l.m11 * r.y + l.m12 * r.z + l.m13 * w,
			z = l.m20 * r.x + l.m21 * r.y + l.m22 * r.z + l.m23 * w,
		};
		
		/// <summary>
		///		Scale the double3x3 part with <paramref name="s"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4x4 scale3x3(double4x4 m, double s) => new double4x4
		{
			m00 = m.m00 * s, m01 = m.m01 * s, m02 = m.m02 * s, m03 = m.m03,
			m10 = m.m10 * s, m11 = m.m11 * s, m12 = m.m12 * s, m13 = m.m13,
			m20 = m.m20 * s, m21 = m.m21 * s, m22 = m.m22 * s, m23 = m.m23,
			m30 = m.m30, m31 = m.m31, m32 = m.m32, m33 = m.m33,
		};

		/// <summary>
		///		Convert to Json array.
		/// </summary>
		public Json.Array ToJson() => new Json.Array
		{
			m00, m01, m02, m03,
			m10, m11, m12, m13,
			m20, m21, m22, m23,
			m30, m31, m32, m33,
		};
	}
}
