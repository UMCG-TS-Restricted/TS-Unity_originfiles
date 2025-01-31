using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct ForcePlate
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0xF07A;

			/// <summary>Force in Vicon transform space.</summary>
			public double3 vicon_force;

			/// <summary>Moment in Vicon transform space.</summary>
			public double3 vicon_moment;

			/// <summary>Center of pressure in Vicon transform space.</summary>
			public double3 vicon_cop;

			/// <summary>Cached force in unity space</summary>
			/// <remarks>Not serialized; the value can be reconstructed by State::vector_transform * vicon_force</remarks>
			public double3 unity_force;

			/// <summary>Cached moment in unity space</summary>
			/// <remarks>Not serialized; the value can be reconstructed by State::vector_transform * vicon_moment</remarks>
			public double3 unity_moment;

			/// <summary>Cached center of pressure in unity space</summary>
			/// <remarks>Not serialized; the value can be reconstructed by State::vector_transform * vicon_cop</remarks>
			public double3 unity_cop;

			/// <summary>Flag if the force value is valid [0: invalid; otherwise: valid]</summary>
			public byte valid_force;

			/// <summary>Flag if the moment value is valid [0: invalid; otherwise: valid]</summary>
			public byte valid_moment;

			/// <summary>Flag if the center of pressure value is valid [0: invalid; otherwise: valid]</summary>
			public byte valid_cop;


			/// <summary>
			///		Serializes <paramref name="plate"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry writer, ForcePlate plate)
			{
				writer.Begin(TypeId, Telemetry.FlagBody, 1);
				writer.Write(plate.vicon_force);
				writer.Write(plate.vicon_moment);
				writer.Write(plate.vicon_cop);
				writer.Write(plate.valid_force);
				writer.Write(plate.valid_moment);
				writer.Write(plate.valid_cop);
				writer.End();
			}

			/// <summary>
			///		Deserializes from <paramref name="value"/> and stores it in <paramref name="plate"/>.
			///		If deserializing fails it will return false, if successful it will return true.
			/// </summary>
			public static bool Read(telemetry.Object value, double3x3 vector_transform, out ForcePlate plate)
			{
				plate = default;
				if (value.type != TypeId)
					return false;

				var reader = value.Read(default);
				reader = reader.read(out plate.vicon_force);
				reader = reader.read(out plate.vicon_moment);
				reader = reader.read(out plate.vicon_cop);
				reader = reader.read(out plate.valid_force);
				reader = reader.read(out plate.valid_moment);
				reader = reader.read(out plate.valid_cop);

				plate.unity_force = vector_transform * plate.vicon_force;
				plate.unity_moment = vector_transform * plate.vicon_moment;
				plate.unity_cop = vector_transform * plate.vicon_cop;

				return true;
			}

			/// <summary>
			///		Convert to Json.
			/// </summary>
			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["header"] = TypeId;
				obj["type"] = "force_plate";

				obj["vicon_force"] = vicon_force.json();
				obj["unity_force"] = unity_force.json();
				obj["valid_force"] = valid_force;

				obj["vicon_moment"] = vicon_moment.json();
				obj["unity_moment"] = unity_moment.json();
				obj["valid_moment"] = valid_moment;

				obj["vicon_cop"] = vicon_cop.json();
				obj["unity_cop"] = unity_cop.json();
				obj["valid_cop"] = valid_cop;
				return obj;
			}
		}
	}
}
