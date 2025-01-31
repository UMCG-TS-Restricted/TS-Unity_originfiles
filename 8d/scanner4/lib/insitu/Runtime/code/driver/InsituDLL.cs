using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace insitu
{
	public static class InsituDLL
	{
		public const string DLL = "insitu";

		/// <summary>
		///		Perform singular value decomposition and combine the result to retrieve only the rotation matrix.
		/// </summary>
		/// <remarks>
		///		This uses the Eigen library - only available as a C library.
		///		See the plug-in for the implementation.
		/// </remarks>
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void svd([In] ref double3x3 input, [Out] out double3x3 output);


		[Obsolete("This specific configuration is not used. Reference and positions should be obtained directly from the source.")]
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

				if (valid_vertices > 0)
				{
					reference_centroid /= valid_vertices;
					current_centroid /= valid_vertices;
				}
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

			svd(ref correlation, out var m);
			return new double4x4
			{
				m00 = m.m00,
				m01 = m.m01,
				m02 = m.m02,
				m03 = current_centroid.x,
				m10 = m.m10,
				m11 = m.m11,
				m12 = m.m12,
				m13 = current_centroid.y,
				m20 = m.m20,
				m21 = m.m21,
				m22 = m.m22,
				m23 = current_centroid.z,
				m30 = 0.0f,
				m31 = 0.0f,
				m32 = 0.0f,
				m33 = 1.0f,
			};
		}
	}
}