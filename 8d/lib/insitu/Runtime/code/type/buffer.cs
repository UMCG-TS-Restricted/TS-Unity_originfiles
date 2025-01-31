using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;


namespace insitu
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct buffer
	{
		public const ushort sizeof_byte = 1;
		public const ushort sizeof_v3 = 3 * sizeof(float);
		public const ushort sizeof_v4 = 4 * sizeof(float);
		public const ushort sizeof_quaternion = 4 * sizeof(float);


		public int size;
		public byte* value;

		public void memset()
		{
			for (var i = 0; i < size; i++)
				value[i] = 0;
		}

		public static unsafe byte* write<T>(byte* buffer, T arg) where T : unmanaged
		{
			var result = (byte*)&arg;
			for (var i = 0; i < sizeof(T); i++)
				buffer[i] = result[i];

			return buffer + sizeof(T);
		}

		public static unsafe byte* write(byte* buffer, string arg, int flags, int max_length)
		{
			fixed (char* src = arg)
			{
				var start = buffer + 8;
				var length = Encoding.UTF8.GetBytes(src, arg.Length, start, max_length);
				write(buffer + 0, length);
				write(buffer + 4, flags);
				buffer = start + length;
			}
			return buffer;
		}

		public static byte* write(byte* buffer, double3 arg)
		{
			buffer = write(buffer, arg.x);
			buffer = write(buffer, arg.y);
			buffer = write(buffer, arg.z);
			return buffer;
		}

		public static byte* write(byte* buffer, double4 arg)
		{
			buffer = write(buffer, arg.x);
			buffer = write(buffer, arg.y);
			buffer = write(buffer, arg.z);
			buffer = write(buffer, arg.w);
			return buffer;
		}

		public static byte* write(byte* buffer, Vector3 arg)
		{
			buffer = write(buffer, arg.x);
			buffer = write(buffer, arg.y);
			buffer = write(buffer, arg.z);
			return buffer;
		}

		public static byte* write(byte* buffer, Quaternion arg)
		{
			buffer = write(buffer, arg.x);
			buffer = write(buffer, arg.y);
			buffer = write(buffer, arg.z);
			buffer = write(buffer, arg.w);
			return buffer;
		}

		public static byte* write_header(byte* buffer, ushort header, byte flags, byte version)
		{
			buffer = write<ushort>(buffer, header);
			buffer = write<byte>(buffer, flags);
			buffer = write<byte>(buffer, version);
			return buffer;
		}

		public static byte* write_entity(byte* buffer, int id)
		{
			buffer = write(buffer, id);
			return buffer;
		}

		public static byte* write_group(byte* buffer, ushort count)
		{
			buffer = write(buffer, count);
			return buffer;
		}

		public static byte* write_group(byte* buffer, byte* start, out int current)
		{
			current = (int)(buffer - start);
			buffer = write<ushort>(buffer, (ushort)0);
			return buffer;
		}

		public static byte* write_body(byte* buffer, int length)
		{
			buffer = write(buffer, length);
			return buffer;
		}

		public static byte* write_body(byte* buffer, byte* start, out int current)
		{
			current = (int)(buffer - start);
			buffer = write<int>(buffer, (int)0);
			return buffer;
		}

		public static unsafe T read<T>(byte* buffer, bool endian) where T : unmanaged
		{
			T value;
			var result = (byte*)&value;
			if (endian)
			{
				for (var i = 0; i < sizeof(T); i++)
					result[sizeof(T) - 1 - i] = buffer[i];
			}
			else
			{
				for (var i = 0; i < sizeof(T); i++)
					result[i] = buffer[i];
			}
			return value;
		}

		public static byte* read<T>(byte* buffer, bool endian, out T arg) where T : unmanaged
		{
			T value;
			var result = (byte*)&value;
			if (endian)
			{
				for (var i = 0; i < sizeof(T); i++)
					result[sizeof(T) - 1 - i] = buffer[i];
			}
			else
			{
				for (var i = 0; i < sizeof(T); i++)
					result[i] = buffer[i];
			}

			arg = value;
			return buffer + sizeof(T);
		}

		public static byte* read(byte* buffer, bool endian, out string arg, out int flags)
		{
			buffer = read(buffer, endian, out int length);
			buffer = read(buffer, endian, out flags);
			arg = Encoding.UTF8.GetString(buffer, length);
			buffer += length;
			return buffer;
		}

		public static byte* read(byte* buffer, bool endian, out double3 arg)
		{
			buffer = read(buffer, endian, out arg.x);
			buffer = read(buffer, endian, out arg.y);
			buffer = read(buffer, endian, out arg.z);
			return buffer;
		}

		public static byte* read(byte* buffer, bool endian, out double4 arg)
		{
			buffer = read(buffer, endian, out arg.x);
			buffer = read(buffer, endian, out arg.y);
			buffer = read(buffer, endian, out arg.z);
			buffer = read(buffer, endian, out arg.w);
			return buffer;
		}

		public static byte* read(byte* buffer, bool endian, out Vector3 arg)
		{
			buffer = read(buffer, endian, out arg.x);
			buffer = read(buffer, endian, out arg.y);
			buffer = read(buffer, endian, out arg.z);
			return buffer;
		}

		public static byte* read(byte* buffer, bool endian, out Quaternion arg)
		{
			buffer = read(buffer, endian, out arg.x);
			buffer = read(buffer, endian, out arg.y);
			buffer = read(buffer, endian, out arg.z);
			buffer = read(buffer, endian, out arg.w);
			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Differs(int a, int b, int tollerence = 0) => Math.Abs(a - b) > tollerence;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Differs(float a, float b, float tollerence = 0.001f) => Math.Abs(a - b) > tollerence;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] 
		public static bool Differs(Vector3 a, Vector3 b, float tollerence = 0.001f) => (a - b).sqrMagnitude > tollerence;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] 
		public static bool Differs(Quaternion a, Quaternion b, float tollerence = 0.0001f) => new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w).sqrMagnitude > tollerence;
	}
}
