using System.IO;
using System.Security;
using System.Text;


namespace insitu.telemetry
{
	/// <summary>
	///		Used to write data to a file.
	///		When serializing an object, first call Begin.
	///		After which you can use the Write commands.
	/// </summary>
	public readonly struct FileWriter
	{
		public readonly Stream Stream;
		public readonly byte[] Cache;

		/// <example>
		///		var cache = new byte[cache_size];
		///		var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
		///		var writer = new FileWriter(stream, cache);
		/// </example>
		public FileWriter(Stream stream, byte[] cache)
		{
			Stream = stream;
			Cache = cache;
		}

		[SecurityCritical]
		public unsafe void Write<T>(T value) where T : unmanaged
		{
			var bytes = Cache;
			fixed (byte* ptr = bytes)
			{
				buffer.write(ptr, value);
				Stream.Write(bytes, 0, sizeof(T));
			}
		}

		[SecurityCritical]
		public unsafe void Write(string value, byte[] bytes = null)
		{
			bytes ??= Cache;

			var length = 0;
			fixed (byte* result = bytes)
			fixed (char* src = value)
			{
				length = Encoding.UTF8.GetBytes(src, value.Length, result + 4, bytes.Length - 4);
				buffer.write(result, length);
			}

			Stream.Write(bytes, 0, length + 4);
		}

		/// <summary>
		///		Mark the serialization of a new object.
		/// </summary>
		/// <param name="type">Type identifier</param>
		/// <param name="flags">Flags to determine the features used in the serialized object.</param>
		/// <param name="version">Serialization version</param>
		/// <param name="id">Entity id; the FlagEntity has to be on to take effect.</param>
		/// <param name="length">
		///		The length of the body; the FlagBody has to be on to take effect.
		///		When length is < 0, the length isn't written.
		///		Make sure to write the length directly afterwards, as it is used in reading the file.
		/// </param>
		public void Begin(ushort type, byte flags, byte version, int id = 0, int length = 0)
		{
			Write(type);
			Write(flags);
			Write(version);

			if ((flags & Telemetry.FlagEntity) != 0)
				Write(id);

			if (length >= 0 && (flags & Telemetry.FlagBody) != 0)
				Write(length);
		}
	}
}
