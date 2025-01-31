using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		A collection of simple hashing procedures.
	/// </summary>
	public static class Hash
	{
		/// <summary>
		///		A union of 32-bit value types.
		///		Used to bit-wise convert between values.
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		public struct dword
		{
			/// <summary>32-bit floating point value.</summary>
			[FieldOffset(0)] public float f;
			/// <summary>32-bit signed integer value.</summary>
			[FieldOffset(0)] public int i;
			/// <summary>32-bit unsigned integer value.</summary>
			[FieldOffset(0)] public uint u;


			/// <summary>Bit-wise convert a 32-bit unsigned integer to a 32-bit floating point value.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static float utf(uint x) => new dword { u = x }.f;
			/// <summary>Bit-wise convert a 32-bit signed integer to a 32-bit floating point value.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static float itf(int x) => new dword { i = x }.f;
			/// <summary>Bit-wise convert a 32-bit floating point value to a 32-bit unsigned integer.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static uint ftu(float x) => new dword { f = x }.u;
			/// <summary>Bit-wise convert a 32-bit floating point value to a 32-bit signed integer.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int fti(float x) => new dword { f = x }.i;
			/// <summary>Bit-wise convert a 32-bit signed integer value to a 32-bit unsigned integer.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static uint itu(int x) => new dword { i = x }.u;
			/// <summary>Bit-wise convert a 32-bit signed integer value to a 32-bit unsigned integer.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int uti(uint x) => new dword { u = x }.i;
		}


		/// <summary>
		///		A simple and relatively fast hash function.
		///		Useful for generating reproducible random values.
		/// </summary>
		/// <remarks>DO NOT USE THIS FOR CRYPTOGRAPHIC PURPOSES.</remarks>
		/// <param name="m">Value that needs hashing.</param>
		/// <param name="seed">A number - preferably a prime - which offsets the input value.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Simple(uint m, uint seed)
		{
			unchecked
			{
				m = (m * 0x68E31DA4) + seed;
				m = (m ^ (m >> 8)) + 0xB5297A4D;
				m = (m ^ (m << 8)) * 0x1B56C4E9;
				return m ^ (m >> 8);
			}
		}

		/// <summary>
		///		Generate a float value (0 [inclusive] .. 1 [exclusive]) based on the seed of the input value.
		/// </summary>
		/// <param name="v">Value that needs hashing.</param>
		/// <param name="seed">A number - preferably a prime - which offsets the input value.</param>
		/// <returns>A value between 0 [inclusive] and 1 [exclusive]</returns>
		/// <seealso cref="Simple(uint, uint)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Noise(uint v, uint seed)
		{
			uint x = 0x3f800000 | (0x007fffff & Simple(v, seed));
			return dword.utf(x) - 1;
		}

		/// <summary>
		///		Generate a float value (0 [inclusive] .. 1 [exclusive]) based on the seed of the input value.
		/// </summary>
		/// <param name="v">Value that needs hashing. The remainder is used to interpolate between the floor- and ceil value.</param>
		/// <param name="seed">A number - preferably a prime - which offsets the input value.</param>
		/// <returns>A value between 0 [inclusive] and 1 [exclusive]</returns>
		/// <seealso cref="Noise(uint, uint)"/>
		/// <seealso cref="Simple(uint, uint)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Noise(float v, uint seed)
		{
			int xmin = v < 0 ? (int)(v - 1) : (int)v;
			var xvar = Noise(xmin, seed);
			return Mathf.LerpUnclamped(xvar, Noise(xmin + 1, seed), v - xmin);
		}
	}
}
