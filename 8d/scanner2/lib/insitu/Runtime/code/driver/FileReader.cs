using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace insitu.telemetry
{
	/// <summary>
	///		Read a slice{byte} and automatically advance when reading a value.
	/// </summary>
	/// <example>
	///		reader = reader.read_cached(out device.name);
	///		reader = reader.read(out device.outputs.offset);
	///		reader = reader.read(out device.outputs.length);
	/// </example>
	public struct FileReader
	{
		public slice<byte> data;
		public array<string> string_cache;
		public bool swap_endianness;

		public static unsafe FileReader read_<T>(FileReader reader, out T arg) where T : unmanaged
		{
			T value;
			var buffer = reader.data;
			fixed (byte* source = buffer)
			{
				var result = (byte*)&value;
				if (reader.swap_endianness)
				{
					for (var i = 0; i < sizeof(T); i++)
						result[sizeof(T) - i - 1] = buffer[i];
				}
				else
				{
					for (var i = 0; i < sizeof(T); i++)
						result[i] = buffer[i];
				}
			}

			arg = value;
			buffer.offset += sizeof(T);
			buffer.length -= sizeof(T);
			reader.data = buffer;
			return reader;
		}

		public static unsafe FileReader read_(FileReader reader, out string arg)
		{
			reader = read_(reader, out int length);

			string value;
			var buffer = reader.data;
			fixed (byte* source = buffer)
				value = Encoding.UTF8.GetString(source, length);

			arg = value;
			buffer.offset += length;
			buffer.length -= length;
			reader.data = buffer;
			return reader;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out byte value) => read_(this, out value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out ushort value) => read_(this, out value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out int value) => read_(this, out value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out uint value) => read_(this, out value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out float value) => read_(this, out value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out double value) => read_(this, out value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out Vector3 value)
		{
			FileReader reader = this;
			reader = read_(reader, out value.x);
			reader = read_(reader, out value.y);
			reader = read_(reader, out value.z);
			return reader;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out Vector4 value)
		{
			FileReader reader = this;
			reader = read_(reader, out value.x);
			reader = read_(reader, out value.y);
			reader = read_(reader, out value.z);
			reader = read_(reader, out value.w);
			return reader;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out Quaternion value)
		{
			FileReader reader = this;
			reader = read_(reader, out value.x);
			reader = read_(reader, out value.y);
			reader = read_(reader, out value.z);
			reader = read_(reader, out value.w);
			return reader;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out Color value)
		{
			FileReader reader = this;
			reader = read_(reader, out value.r);
			reader = read_(reader, out value.g);
			reader = read_(reader, out value.b);
			reader = read_(reader, out value.a);
			return reader;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out double3 value)
		{
			FileReader reader = this;
			reader = read_(reader, out value.x);
			reader = read_(reader, out value.y);
			reader = read_(reader, out value.z);
			return reader;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out double4 value)
		{
			FileReader reader = this;
			reader = read_(reader, out value.x);
			reader = read_(reader, out value.y);
			reader = read_(reader, out value.z);
			reader = read_(reader, out value.w);
			return reader;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileReader read(out double4x4 value)
		{
			FileReader reader = this;

			reader = read_(reader, out value.m00);
			reader = read_(reader, out value.m01);
			reader = read_(reader, out value.m02);
			reader = read_(reader, out value.m03);

			reader = read_(reader, out value.m10);
			reader = read_(reader, out value.m11);
			reader = read_(reader, out value.m12);
			reader = read_(reader, out value.m13);

			reader = read_(reader, out value.m20);
			reader = read_(reader, out value.m21);
			reader = read_(reader, out value.m22);
			reader = read_(reader, out value.m23);

			reader = read_(reader, out value.m30);
			reader = read_(reader, out value.m31);
			reader = read_(reader, out value.m32);
			reader = read_(reader, out value.m33);

			return reader;
		}

		public FileReader read(out string value) => read_(this, out value);

		/// <summary>
		///		Read integer and lookup in string_cache.
		///		If the index is out of range <paramref name="value"/> will get a null value.
		/// </summary>
		public FileReader read_cached(out string value)
		{
			var reader = this;
			reader = read_(reader, out int index);
			value = string_cache.At(index);
			return reader;
		}
	}
}
