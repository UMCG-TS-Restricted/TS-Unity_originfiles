using System.Text;

namespace insitu.telemetry
{
	/// <summary>
	///		Descriptor of a field to convert binary data to another file format.
	/// </summary>
	public struct FieldInfo
	{
		/// <summary>
		///		Serialization type identifier.
		/// </summary>
		public const ushort TypeId = 0x0E02;

		/// <summary>
		///		Type identifier of the field type.
		/// </summary>
		/// <seealso cref="TypeInfo.type"/>
		public ushort type;

		/// <summary>
		///		Name of the field.
		/// </summary>
		public string name;

		/// <summary>
		///		Serializes <paramref name="field"/> to <paramref name="writer"/>.
		/// </summary>
		public static void Write(FileWriter writer, FieldInfo field)
		{
			var name = Encoding.UTF8.GetBytes(field.name);
			var total_length = name.Length + 6;
			writer.Begin(TypeId, Telemetry.FlagBody, 1, length: total_length);
			writer.Write(field.type);
			writer.Write(name.Length);
			writer.Stream.Write(name);
		}

		public static bool Read(Object obj, out FieldInfo field)
		{
			field = default;
			if (obj.type != TypeId)
				return false;

			var reader = obj.Read(default);
			reader = reader.read(out field.type);
			reader = reader.read(out field.name);
			return true;
		}
	}
}
