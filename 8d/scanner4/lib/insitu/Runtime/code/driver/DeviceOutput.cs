using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct DeviceOutput
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0xDE10;

			/// <summary>
			///		Name of the output field.
			/// </summary>
			public string name;

			/// <summary>
			///		Value of the output field.
			/// </summary>
			public double value;

			/// <summary>
			///		Unit type of the output field.
			///		Use Units.ClampedAt(unit) to convert the type to a readable string.
			/// </summary>
			/// <seealso cref="Units"/>
			public int unit;

			/// <summary>
			///		0: The value is invalid.
			///		Otherwise the value is valid.
			/// </summary>
			public byte valid;

			/// <summary>
			///		Convert to Json.
			/// </summary>
			public readonly Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["header"] = TypeId;
				obj["type"] = "device_output";
				obj["name"] = name ?? string.Empty;
				obj["unit"] = unit;
				obj["unit_name"] = Units.ClampedAt(unit);
				obj["value"] = value;
				obj["valid"] = valid;
				return obj;
			}

			/// <summary>
			///		Serializes <paramref name="output"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry telemetry, DeviceOutput output)
			{
				telemetry.Begin(TypeId, Telemetry.FlagBody, 1);
				telemetry.WriteCached(output.name);
				telemetry.Write(output.unit);
				telemetry.Write(output.value);
				telemetry.Write(output.valid);
				telemetry.End();
			}

			/// <summary>
			///		Deserializes from <paramref name="value"/> and stores it in <paramref name="device"/>.
			///		If deserializing fails it will return false, if successful it will return true.
			/// </summary>
			public static bool Read(telemetry.Object value, array<string> string_cache, out DeviceOutput output)
			{
				output = default;
				if (value.type != TypeId)
					return false;

				var reader = value.Read(string_cache);
				reader = reader.read_cached(out output.name);
				reader = reader.read(out output.unit);
				reader = reader.read(out output.value);
				reader = reader.read(out output.valid);
				return true;
			}

			public static readonly string[] Units =
			{
				"Unknown",
				"Volt",
				"Newton",
				"NewtonMeter",
				"Meter",
				"Kilogram",
				"Second",
				"Ampere",
				"Kelvin",
				"Mole",
				"Candela",
				"Radian",
				"Steradian",
				"MeterSquared",
				"MeterCubed",
				"MeterPerSecond",
				"MeterPerSecondSquared",
				"RadianPerSecond",
				"RadianPerSecondSquared",
				"Hertz",
				"Joule",
				"Watt",
				"Pascal",
				"Lumen",
				"Lux",
				"Coulomb",
				"Ohm",
				"Farad",
				"Weber",
				"Tesla",
				"Henry",
				"Siemens",
				"Becquerel",
				"Gray",
				"Sievert",
				"Katal",
				"Unknown",
			};
		}
	}
}
