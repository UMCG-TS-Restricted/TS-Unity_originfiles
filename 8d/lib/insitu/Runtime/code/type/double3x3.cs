using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace insitu
{
	/// <summary>
	///		A standard 3x3 matrix.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct double3x3
	{
		public double m00;
		public double m01;
		public double m02;

		public double m10;
		public double m11;
		public double m12;

		public double m20;
		public double m21;
		public double m22;


		/// <summary>
		///		Perform singular value decomposition and combine the result to retrieve only the rotation matrix.
		/// </summary>
		/// <remarks>
		///		This uses the Eigen library - only available as a C library.
		///		See the plug-in for the implementation.
		/// </remarks>
		[DllImport("insitu", CallingConvention = CallingConvention.Cdecl)]
		public static extern void svd([In] ref double3x3 input, [Out] out double3x3 output);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 operator +(double3x3 l, double3x3 r) => new double3x3
		{
			m00 = l.m00 + r.m00, m01 = l.m01 + r.m01, m02 = l.m02 + r.m02,
			m10 = l.m10 + r.m10, m11 = l.m11 + r.m11, m12 = l.m12 + r.m12,
			m20 = l.m20 + r.m20, m21 = l.m21 + r.m21, m22 = l.m22 + r.m22,
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator *(double3x3 l, double3 r) => new double3
		{
			x = l.m00 * r.x + l.m01 * r.y + l.m02 * r.z,
			y = l.m10 * r.x + l.m11 * r.y + l.m12 * r.z,
			z = l.m20 * r.x + l.m21 * r.y + l.m22 * r.z,
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4x4 m4x4(double3x3 m, double3 p) => new Matrix4x4
		{
			m00 = (float)m.m00, m01 = (float)m.m01, m02 = (float)m.m02, m03 = (float)p.x,
			m10 = (float)m.m10, m11 = (float)m.m11, m12 = (float)m.m12, m13 = (float)p.y,
			m20 = (float)m.m20, m21 = (float)m.m21, m22 = (float)m.m22, m23 = (float)p.z,
			m30 =         0.0f, m31 =         0.0f, m32 =         0.0f, m33 =       1.0f,
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 transpose(double3x3 m) => new double3x3
		{
			m00 = m.m00, m01 = m.m10, m02 = m.m20,
			m10 = m.m01, m11 = m.m11, m12 = m.m21,
			m20 = m.m02, m21 = m.m12, m22 = m.m22,
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 outer(double3 a, double3 b) => new double3x3
		{
			m00 = a.x * b.x, m01 = a.x * b.y, m02 = a.x * b.z,
			m10 = a.y * b.x, m11 = a.y * b.y, m12 = a.y * b.z,
			m20 = a.z * b.x, m21 = a.z * b.y, m22 = a.z * b.z,
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double determinant(double3x3 m) =>
			m.m00 * (m.m11 * m.m22 - m.m12 * m.m21) -
			m.m01 * (m.m10 * m.m22 - m.m12 * m.m20) +
			m.m02 * (m.m10 * m.m21 - m.m11 * m.m20);
	}
}
