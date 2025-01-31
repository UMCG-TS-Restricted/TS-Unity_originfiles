namespace insitu.telemetry
{
	/// <summary>
	///		First state of an object after deserialisation.
	/// </summary>
	public struct Object
	{
		public ushort type;
		public byte flags;
		public byte version;
		public int entity;
		public int self_index;
		public int previous_index;
		public int next_index;
		public int frame_index;
		public slice<byte> data;
		public bool swap_endian;

		public readonly bool is_entity => (flags & Telemetry.FlagEntity) != 0;
		public readonly bool has_body => (flags & Telemetry.FlagBody) != 0;

		/// <summary>
		///		Creates a file reader for reading the body data.
		/// </summary>
		/// <param name="string_cache">If no cached strings are used, you can use an empty array</param>
		public FileReader Read(array<string> string_cache) => new FileReader
		{
			data = data,
			string_cache = string_cache,
			swap_endianness = swap_endian,
		};
	}
}
