using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using System;


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
		
		public double4 rotation()
		{
			var tr = m00 + m11 + m22;
			if (tr > 0)
			{
				var s = Math.Sqrt(tr + 1.0) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = (m21 - m12) * si,
					y = (m02 - m20) * si,
					z = (m10 - m01) * si,
					w = 0.25 * s,
				};
			}
			else if ((m00 > m11) & (m00 > m22))
			{
				var s = Math.Sqrt(1.0 + m00 - m11 - m22) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = 0.25 * s,
					y = (m01 + m10) * si,
					z = (m02 + m20) * si,
					w = (m21 - m12) * si,
				};
			}
			else if (m11 > m22)
			{
				var s = Math.Sqrt(1.0 + m11 - m00 - m22) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = (m01 + m10) * si,
					y = 0.25 * s,
					z = (m12 + m21) * si,
					w = (m02 - m20) * si,
				};
			}
			else
			{
				var s = Math.Sqrt(1.0 + m22 - m00 - m11) * 2;
				var si = 1.0 / s;
				return new double4
				{
					x = (m02 + m20) * si,
					y = (m12 + m21) * si,
					z = 0.25 * s,
					w = (m10 - m01) * si,
				};
			}
		}

		public double3 position() => new double3 { x = m03, y = m13, z = m23 };


		public static double4x4 transformed_by(int length, array<double4> reference, array<double4> positions)
		{
			Debug.Assert(length <= reference.length);
			Debug.Assert(length <= positions.length);
			
			// Compute centroid
			var reference_centroid = new double3 { };
			var current_centroid = new double3 { };
			{
				var valid_vertices = 0;
				for (var i = 0; i < length; i++)
				{
					if (reference[i].w < 1 || positions[i].w < 1)
						continue;
					
					reference_centroid += reference[i].d3();
					current_centroid += positions[i].d3();
					valid_vertices++;
				}
				reference_centroid /= valid_vertices;
				current_centroid /= valid_vertices;
			}
			
			// Compute correlation
			var correlation = new double3x3 { };
			for (var i = 0; i < length; i++)
			{
				if (reference[i].w < 1 || positions[i].w < 1)
					continue;

				var r = reference[i];
				var c = positions[i];
				var ri = new double3
				{
					x = r.x - reference_centroid.x,
					y = r.y - reference_centroid.y,
					z = r.z - reference_centroid.z,
				};
				var ci = new double3
				{
					x = c.x - current_centroid.x,
					y = c.y - current_centroid.y,
					z = c.z - current_centroid.z,
				};
				correlation = correlation + double3x3.outer(ri, ci);
			}
			
			double3x3.svd(ref correlation, out var m);
			return new double4x4
			{
				m00 = m.m00, m01 = m.m01, m02 = m.m02, m03 = current_centroid.x,
				m10 = m.m10, m11 = m.m11, m12 = m.m12, m13 = current_centroid.y,
				m20 = m.m20, m21 = m.m21, m22 = m.m22, m23 = current_centroid.z,
				m30 =  0.0f, m31 =  0.0f, m32 =  0.0f, m33 = 1.0f,
			};
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4x4 m4x4(double4x4 m) => new Matrix4x4
		{
			m00 = (float)m.m00, m01 = (float)m.m01, m02 = (float)m.m02, m03 = (float)m.m03,
			m10 = (float)m.m10, m11 = (float)m.m11, m12 = (float)m.m12, m13 = (float)m.m13,
			m20 = (float)m.m20, m21 = (float)m.m21, m22 = (float)m.m22, m23 = (float)m.m23,
			m30 = (float)m.m30, m31 = (float)m.m31, m32 = (float)m.m32, m33 = (float)m.m33,
		};
	}
}
