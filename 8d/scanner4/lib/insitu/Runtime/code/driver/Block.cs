using insitu.memory;

namespace insitu.telemetry
{
	/// <summary>
	///		A collection of frames using a dynamically growing buffer.
	///		Each block resets the accumulative state.
	///		The amount of frames should be limited as it has to read a full block in order to determine the state of a given frame.
	/// </summary>
	public struct Block
	{
		/// <summary>
		///		Serialization type identifier.
		/// </summary>
		public const ushort TypeId = 0xB10C;

		public ushort frame_count;
		public Pool frames;

		/// <summary>
		///		Serializes <paramref name="block"/> into <paramref name="writer"/>.
		/// </summary>
		public static void Write(FileWriter writer, Block block)
		{
			writer.Begin(TypeId, 0, 1);
			var frames = block.frames;
			if (frames != null && frames.head != null)
			{
				var current = frames.tail;
				while (current != null)
				{
					writer.Stream.Write(current.data, 0, current.length);
					if (current == frames.head)
						break;

					current = current.next;
				}
			}
		}

		public static bool Read(Object obj) => obj.type == TypeId;
	}
}
