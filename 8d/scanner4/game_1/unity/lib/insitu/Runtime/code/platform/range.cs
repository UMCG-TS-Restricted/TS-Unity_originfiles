namespace insitu
{
	/// <summary>
	///		A range of an array without its reference.
	///		Useful for a serializable structure that is not allowed to have pointers.
	/// </summary>
	/// <seealso cref="insitu.slice{T}"/>
	public struct range
	{
		public int offset;
		public int length;

		public range(int offset, int length)
		{
			this.offset = offset;
			this.length = length;
		}

		/// <summary>
		///		Combines itself with <paramref name="elements"/>
		/// </summary>
		public slice<T> slice<T>(T[] elements) => new slice<T>
		{
			elements = elements,
			offset = offset,
			length = length,
		};
	}
}
