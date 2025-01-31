using System.Runtime.CompilerServices;


namespace insitu
{
	/// <summary>
	///		A lightweight implementation of ArraySegment.
	///		Useful for avoiding scattered memory.
	/// </summary>
	/// <seealso cref="insitu.range"/>
	public struct slice<T>
	{
		public T[] elements;
		public int offset;
		public int length;

		/// <summary>
		///		Shorthand for elements[offset + <paramref name="index"/>].
		/// </summary>
		/// <remarks>
		///		There are no safety/bound checks.
		/// </remarks>
		public readonly T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => elements[offset + index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => elements[offset + index] = value;
		}

		/// <summary>
		///		Return itself without elements.
		/// </summary>
		public range range => new range
		{
			offset = offset,
			length = length,
		};

		/// <summary>Implicitly gets used in fixed statement.</summary>
		public ref T GetPinnableReference() => ref elements[offset];

		public static slice<T> from(T[] array) => new slice<T>
		{
			elements = array,
			offset = 0,
			length = array.Length,
		};
	}
}
