using System.Text;
using ADG;


namespace insitu.telemetry
{
	public struct JsonData
	{
		/// <summary>
		///		Serialization type identifier.
		/// </summary>
		public const ushort TypeId = 0x67AD;

		/// <summary>
		///		Write Json Object
		/// </summary>
		public static void Write(FileWriter writer, string value)
		{
			var content = Encoding.UTF8.GetBytes(value);
			writer.Begin(TypeId, Telemetry.FlagBody, 1, length: content.Length);
			writer.Stream.Write(content);
		}

		/// <summary>
		///		Try to convert serialized Object to Json.
		///		returns true when the object type equals the json data type.
		///		This can mean that the parsing of the Json can fail, thus <paramref name="result"/> still has to be checked if it is valid.
		/// </summary>
		public static bool Read(Object obj, out Json.Object result)
		{
			result = null;
			if (obj.type != TypeId)
				return false;

			var data = obj.data;
			var text = Encoding.UTF8.GetString(data.elements, data.offset, data.length);
			result = Json.ParseObject(text);
			return true;
		}
	}
}
