using System.Runtime.InteropServices;


namespace insitu
{
	/// <summary>
	///		An unsafe byte[].
	///		Useful to communicate data in dynamic linked libraries.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct buffer
	{
		public int size;
		public byte* value;

		/// <summary>
		///		Set all bytes to 0.
		/// </summary>
		public void memset()
		{
			for (var i = 0; i < size; i++)
				value[i] = 0;
		}

		/// <summary>
		///		Write a <paramref name="arg"/> to <paramref name="buffer"/>.
		/// </summary>
		/// <remarks>
		///		Use this only with primitives such as: int, float, short, byte.
		///		The <typeparamref name="T"/> is limited to unmanaged types.
		///		However when defining unmanaged structures this is allowed by the compiler,
		///		but can give unexpected results if not careful.
		/// </remarks>
		/// <returns>
		///		<paramref name="buffer"/> + sizeof(<typeparamref name="T"/>).
		/// </returns>
		public static unsafe byte* write<T>(byte* buffer, T arg) where T : unmanaged
		{
			var result = (byte*)&arg;
			for (var i = 0; i < sizeof(T); i++)
				buffer[i] = result[i];

			return buffer + sizeof(T);
		}

		/// <summary>
		///		Write a <paramref name="arg"/> to <paramref name="buffer"/>.
		/// </summary>
		/// <remarks>
		///		Use this only with primitives such as: int, float, short, byte.
		///		The <typeparamref name="T"/> is limited to unmanaged types.
		///		However when defining unmanaged structures this is allowed by the compiler,
		///		but can give unexpected results if not careful.
		/// </remarks>
		/// <returns>
		///		<paramref name="buffer"/> + sizeof(<typeparamref name="T"/>).
		/// </returns>
		public static unsafe int write<T>(slice<byte> buffer, T arg) where T : unmanaged
		{
			fixed (byte* target = buffer)
			{
				var result = (byte*)&arg;
				for (var i = 0; i < sizeof(T); i++)
					buffer[i] = result[i];
			}

			return sizeof(T);
		}
	}
}
