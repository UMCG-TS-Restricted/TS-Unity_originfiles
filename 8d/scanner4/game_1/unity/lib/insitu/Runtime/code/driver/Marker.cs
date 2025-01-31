using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		///		Labeled Marker
		/// </summary>
		public struct Marker
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0xA4E4;

			/// <summary>
			///		Segment parent name
			/// </summary>
			public string parent;

			/// <summary>
			///		Name given to the marker
			/// </summary>
			public string name;

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
			///		Flag if the position value is valid [0: invalid; otherwise: valid]
			///	</summary>
			public byte valid_position;

			/// <summary>
			///		Pose composed of vicon_position.
			/// </summary>
			public readonly pose vicon_pose => new pose
			{
				position = vicon_position,
				valid_position = valid_position,
				rotation = double4.identity,
				valid_rotation = 0,
			};

			/// <summary>
			///		Pose composed of unity_position.
			/// </summary>
			public readonly pose unity_pose => new pose
			{
				position = unity_position,
				valid_position = valid_position,
				rotation = double4.identity,
				valid_rotation = 0,
			};

			/// <summary>
			///		Serializes <paramref name="marker"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry telemetry, Marker marker)
			{
				if(marker.valid_position != 0) { 
				telemetry.Begin(TypeId, Telemetry.FlagBody, 1);
				telemetry.WriteCached(marker.parent);
				telemetry.WriteCached(marker.name);
				telemetry.Write(marker.vicon_position);
				telemetry.Write(marker.valid_position);
				telemetry.End();
                }	
            }

			/// <summary>
			///		Deserializes from <paramref name="value"/> and stores it in <paramref name="marker"/>.
			///		If deserializing fails it will return false, if successful it will return true.
			/// </summary>
			public static bool Read(telemetry.Object value, array<string> string_cache, double3 position_center, double4x4 position_transform, out Marker marker)
			{
				marker = default;
				if (value.type != TypeId)
					return false;

				var reader = value.Read(string_cache);
				reader = reader.read_cached(out marker.parent);
				reader = reader.read_cached(out marker.name);
				reader = reader.read(out marker.vicon_position);
				reader = reader.read(out marker.valid_position);

				marker.unity_position = marker.vicon_position;
				marker.unity_position = marker.unity_position - position_center;
				marker.unity_position = double4x4.mul(position_transform, marker.unity_position, 1.0);
				return true;
			}

			/// <summary>
			///		Convert to Json.
			/// </summary>
			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["header"] = TypeId;
				obj["type"] = "marker";
				obj["parent"] = parent;
				obj["name"] = name;
				obj["vicon_position"] = vicon_position.json();
				obj["unity_position"] = unity_position.json();
				obj["valid"] = valid_position;
				return obj;
			}

			/// <summary>
			///		Convert from Json.
			/// </summary>
			public static Marker FromJson(Json.Object json)
			{
				if (json == null)
					return default;

				var marker = new Marker();
				marker.parent = json["parent"];
				if (marker.parent == null)
					marker.parent = string.Empty;

				marker.name = json["name"];
				if (marker.name == null)
					marker.name = string.Empty;

				marker.vicon_position = double3.from(json.ArrayOf("vicon_position"));
				marker.unity_position = double3.from(json.ArrayOf("unity_position"));
				marker.valid_position = json["valid"];
				return marker;
			}
		}
	}
}
