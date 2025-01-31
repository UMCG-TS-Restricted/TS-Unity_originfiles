using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ADG;


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

		public static readonly double3x3 identity = new double3x3
		{
			m00 = 1, m01 = 0, m02 = 0,
			m10 = 0, m11 = 1, m12 = 0,
			m20 = 0, m21 = 0, m22 = 1,
		};

		/// <summary>
		///		A matrix composed from rows <paramref name="x"/>, <paramref name="y"/> and <paramref name="z"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 from(double3 x, double3 y, double3 z) => new double3x3
		{
			m00 = x.x, m01 = x.y, m02 = x.z,
			m10 = y.x, m11 = y.y, m12 = y.z,
			m20 = z.x, m21 = z.y, m22 = z.z,
		};

		/// <summary>
		///		The inverse of <paramref name="m"/>.
		///		Inverted matrix is such that if multiplied by the original would result in identity matrix.
		/// </summary>
		/// <remarks>
		///		You can not invert a matrix with a determinant of zero.
		///		This will return a zero matrix.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 inverse(double3x3 m)
		{
			var det = m.m00 * (m.m11 * m.m22 - m.m12 * m.m21)
					- m.m01 * (m.m10 * m.m22 - m.m12 * m.m20)
					+ m.m02 * (m.m10 * m.m21 - m.m11 * m.m20);
			
			var mul = Math.Abs(det) <= 0.001 ? 0 : 1 / det;
			return new double3x3
			{
				m00 = mul * (m.m11 * m.m22 - m.m21 * m.m12),
				m01 = mul * (m.m02 * m.m21 - m.m01 * m.m22),
				m02 = mul * (m.m01 * m.m12 - m.m02 * m.m11),
				m10 = mul * (m.m12 * m.m20 - m.m10 * m.m22),
				m11 = mul * (m.m00 * m.m22 - m.m02 * m.m20),
				m12 = mul * (m.m10 * m.m02 - m.m00 * m.m12),
				m20 = mul * (m.m10 * m.m21 - m.m20 * m.m11),
				m21 = mul * (m.m20 * m.m01 - m.m00 * m.m21),
				m22 = mul * (m.m00 * m.m11 - m.m10 * m.m01),
			};
		}

		/// <summary>
		///		Scale <paramref name="m"/> by <paramref name="s"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 scale(double3x3 m, double s) => new double3x3
		{
			m00 = m.m00 * s, m01 = m.m01 * s, m02 = m.m02 * s,
			m10 = m.m10 * s, m11 = m.m11 * s, m12 = m.m12 * s,
			m20 = m.m20 * s, m21 = m.m21 * s, m22 = m.m22 * s,
		};

		/// <summary>
		///		Transform <paramref name="r"/> by <paramref name="l"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 mul(double3x3 l, double3 r) => new double3
		{
			x = l.m00 * r.x + l.m01 * r.y + l.m02 * r.z,
			y = l.m10 * r.x + l.m11 * r.y + l.m12 * r.z,
			z = l.m20 * r.x + l.m21 * r.y + l.m22 * r.z,
		};


		public static double4 rotation2(double3x3 l)
		{
			var tr = l.m00 + l.m11 + l.m22;
			if (tr > 0)
			{
				var s = Math.Sqrt(tr + 1.0) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = (l.m21 - l.m12) * si,
					y = (l.m02 - l.m20) * si,
					z = (l.m10 - l.m01) * si,
					w = 0.25 * s,
				};
			}
			else if ((l.m00 > l.m11) & (l.m00 > l.m22))
			{
				var s = Math.Sqrt(1.0 + l.m00 - l.m11 - l.m22) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = 0.25 * s,
					y = (l.m01 + l.m10) * si,
					z = (l.m02 + l.m20) * si,
					w = (l.m21 - l.m12) * si,
				};
			}
			else if (l.m11 > l.m22)
			{
				var s = Math.Sqrt(1.0 + l.m11 - l.m00 - l.m22) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = (l.m01 + l.m10) * si,
					y = 0.25 * s,
					z = (l.m12 + l.m21) * si,
					w = (l.m02 - l.m20) * si,
				};
			}
			else
			{
				var s = Math.Sqrt(1.0 + l.m22 - l.m00 - l.m11) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = (l.m02 + l.m20) * si,
					y = (l.m12 + l.m21) * si,
					z = 0.25 * s,
					w = (l.m10 - l.m01) * si,
				};
			}
		}

		/// <summary>
		///		Convert the rotation matrix to a quaternion.
		/// </summary>
		public static double4 rotation(double3x3 l)
		{
			var x = rotation2(l);
			return new double4
			{
				x = -x.x,
				y = -x.z,
				z = -x.y,
				w = x.w,
			};
		}

		/// <summary>
		///		Normalize <paramref name="p"/> and only keep the rotation matrix.
		/// </summary>
		public static double3x3 normalized(double4x4 p)
		{
			var v = new double3x3 { };

			var r0 = p.m00 * p.m00 + p.m01 * p.m01 + p.m02 * p.m02;
			if (r0 > 0)
			{
				var scalar = 1.0 / Math.Sqrt(r0);
				v.m00 = p.m00 * scalar;
				v.m01 = p.m01 * scalar;
				v.m02 = p.m02 * scalar;
			}

			var r1 = p.m10 * p.m10 + p.m11 * p.m11 + p.m12 * p.m12;
			if (r1 > 0)
			{
				var scalar = 1.0 / Math.Sqrt(r1);
				v.m10 = p.m10 * scalar;
				v.m11 = p.m11 * scalar;
				v.m12 = p.m12 * scalar;
			}

			var r2 = p.m20 * p.m20 + p.m21 * p.m21 + p.m22 * p.m22;
			if (r2 > 0)
			{
				var scalar = 1.0 / Math.Sqrt(r2);
				v.m20 = p.m20 * scalar;
				v.m21 = p.m21 * scalar;
				v.m22 = p.m22 * scalar;
			}

			return v;
		}

		/// <summary>
		///		The sum of <paramref name="l"/> and <paramref name="r"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 operator +(double3x3 l, double3x3 r) => new double3x3
		{
			m00 = l.m00 + r.m00, m01 = l.m01 + r.m01, m02 = l.m02 + r.m02,
			m10 = l.m10 + r.m10, m11 = l.m11 + r.m11, m12 = l.m12 + r.m12,
			m20 = l.m20 + r.m20, m21 = l.m21 + r.m21, m22 = l.m22 + r.m22,
		};

		/// <summary>
		///		Transform <paramref name="r"/> by <paramref name="l"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 operator *(double3x3 l, double3 r) => new double3
		{
			x = l.m00 * r.x + l.m01 * r.y + l.m02 * r.z,
			y = l.m10 * r.x + l.m11 * r.y + l.m12 * r.z,
			z = l.m20 * r.x + l.m21 * r.y + l.m22 * r.z,
		};

		/// <summary>
		///		Transpose <paramref name="m"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 transpose(double3x3 m) => new double3x3
		{
			m00 = m.m00, m01 = m.m10, m02 = m.m20,
			m10 = m.m01, m11 = m.m11, m12 = m.m21,
			m20 = m.m02, m21 = m.m12, m22 = m.m22,
		};

		/// <summary>
		///		The outer product of <paramref name="a"/> and <paramref name="b"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3x3 outer(double3 a, double3 b) => new double3x3
		{
			m00 = a.x * b.x, m01 = a.x * b.y, m02 = a.x * b.z,
			m10 = a.y * b.x, m11 = a.y * b.y, m12 = a.y * b.z,
			m20 = a.z * b.x, m21 = a.z * b.y, m22 = a.z * b.z,
		};

		/// <summary>
		///		The determinant of the matrix.
		/// </summary>
		/// <remarks>You can not invert matrices with a determinant of zero.</remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double determinant(double3x3 m) =>
			m.m00 * (m.m11 * m.m22 - m.m12 * m.m21) -
			m.m01 * (m.m10 * m.m22 - m.m12 * m.m20) +
			m.m02 * (m.m10 * m.m21 - m.m11 * m.m20);

		/// <summary>
		///		Convert the matrix to a Json array.
		/// </summary>
		public Json.Array ToJson() => new Json.Array
		{
			m00, m01, m02,
			m10, m11, m12,
			m20, m21, m22,
		};
	}
}
