namespace insitu.telemetry
{
	/// <summary>
	///		Serialization frame
	/// </summary>
	public struct Frame
	{
		public const ushort TypeId = 0xF47E;

		public int id;
		public float time;
		public int block_index;
		public range children;

		public static void Write(Telemetry telemetry, int id, float time)
		{
			telemetry.Begin(TypeId, Telemetry.FlagBody, 1);
			telemetry.Write(id);
			telemetry.Write(time);
			telemetry.End();
		}

		public static bool Read(Object obj, out Frame frame)
		{
			frame = default;
			if (obj.type != TypeId)
				return false;

			var reader = obj.Read(default);
			reader = reader.read(out frame.id);
			reader = reader.read(out frame.time);
			return true;
		}
	}
}
