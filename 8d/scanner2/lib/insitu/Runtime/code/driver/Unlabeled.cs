using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		///		Unlabeled Marker
		/// </summary>
		public struct Unlabeled
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0x06AB;

			/// <summary>
			///		Identifier given by Vicon
			/// </summary>
			public uint id;

			/// <summary>
			///		The position of the marker in Vicon global transform space.
			///	</summary>
			public double3 vicon_position;

			/// <summary>
			///		The position of the marker in Vicon global unity space.
			///	</summary>
			/// <remarks>
			///		Not serialized; the value can be reconstructed using double4x4.mul(State::position_transform, vicon_position)
			///	</remarks>
			public double3 unity_position;

			/// <summary>
			///		Compose a pose from vicon_position
			/// </summary>
			public readonly pose vicon_pose => new pose
			{
				position = vicon_position,
				valid_position = 1,
				rotation = double4.identity,
				valid_rotation = 0,
			};

			/// <summary>
			///		Compose a pose from unity_position
			/// </summary>
			public readonly pose unity_pose => new pose
			{
				position = unity_position,
				valid_position = 1,
				rotation = double4.identity,
				valid_rotation = 0,
			};

			/// <summary>
			///		Serializes <paramref name="unlabeled"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry telemetry, Unlabeled unlabeled)
			{
				telemetry.Begin(TypeId, Telemetry.FlagBody, 1);
				telemetry.Write(unlabeled.id);
				telemetry.Write(unlabeled.vicon_position);
				telemetry.End();
			}

			/// <summary>
			///		Deserializes from <paramref name="value"/> and stores it in <paramref name="marker"/>.
			///		If deserializing fails it will return false, if successful it will return true.
			/// </summary>
			public static bool Read(telemetry.Object value, double4x4 position_transform, out Unlabeled marker)
			{
				marker = default;
				if (value.type != TypeId)
					return false;

				var reader = value.Read(default);
				reader = reader.read(out marker.id);
				reader = reader.read(out marker.vicon_position);

				marker.unity_position = double4x4.mul(position_transform, marker.vicon_position, 1.0);
				return true;
			}

			/// <summary>
			///		Convert to Json
			/// </summary>
			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["id"] = id;
				obj["vicon_position"] = vicon_position.json();
				obj["unity_position"] = unity_position.json();
				return obj;
			}

			/// <summary>
			///		Convert from Json.
			/// </summary>
			public static Unlabeled FromJson(Json.Object json)
			{
				if (json == null)
					return default;

				var marker = new Unlabeled();
				marker.id = json["id"];
				marker.vicon_position = double3.from(json.ArrayOf("vicon_position"));
				marker.unity_position = double3.from(json.ArrayOf("unity_position"));
				return marker;
			}

			public override string ToString() => ToJson().ToString();
		}
	}
}
