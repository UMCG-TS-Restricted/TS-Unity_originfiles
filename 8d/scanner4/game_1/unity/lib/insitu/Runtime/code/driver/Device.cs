using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct Device
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0xDE1C;

			/// <summary>
			///		Name of the device.
			/// </summary>
			public string name;

			/// <summary>
			///		References the slice of the outputs array inside State.
			/// </summary>
			/// <see cref="State.outputs"/>
			public range outputs;

			/// <summary>
			///		Serializes <paramref name="device"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry writer, Device device)
			{
				writer.Begin(TypeId, Telemetry.FlagBody, 1);
				writer.WriteCached(device.name);
				writer.Write(device.outputs.offset);
				writer.Write(device.outputs.length);
				writer.End();
			}

			/// <summary>
			///		Deserializes from <paramref name="value"/> and stores it in <paramref name="device"/>.
			///		If deserializing fails it will return false, if successful it will return true.
			/// </summary>
			public static bool Read(telemetry.Object value, array<string> string_cache, out Device device)
			{
				device = default;
				if (value.type != TypeId)
					return false;

				var reader = value.Read(string_cache);
				reader = reader.read_cached(out device.name);
				reader = reader.read(out device.outputs.offset);
				reader = reader.read(out device.outputs.length);
				return true;
			}

			/// <summary>
			///		Convert to Json
			/// </summary>
			public Json.Object ToJson(slice<DeviceOutput> all)
			{
				var obj = new Json.Object();
				obj["header"] = TypeId;
				obj["type"] = "device";
				obj["name"] = name ?? string.Empty;

				var arr = new Json.Array();
				for (var i = 0; i < all.length; i++)
					arr.Add(all[i].ToJson());
				obj["outputs"] = arr;

				return obj;
			}
		}
	}
}
