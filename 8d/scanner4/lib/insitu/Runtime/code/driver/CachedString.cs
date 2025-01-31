using ADG;
using System.Text;

namespace insitu.telemetry
{
	/// <summary>
	///		Structure information of cached strings.
	/// </summary>
	public static class CachedString
	{
		/// <summary>
		///		Serialization type identifier.
		/// </summary>
		public const ushort TypeId = 0xCA51;

		/// <see cref="FileWriter.Write(string, byte[])"/>
		public static void Write(FileWriter writer, string value, byte[] cache = null)
		{
			writer.Begin(TypeId, Telemetry.FlagBody, 1, length: -1);
			writer.Write(value, cache);
		}

		public static bool Read(Object obj, out string value)
		{
			value = null;
			if (obj.type != TypeId)
				return false;

			var data = obj.data;
			value = Encoding.UTF8.GetString(data.elements, data.offset, data.length);
			return true;
		}
	}
}
