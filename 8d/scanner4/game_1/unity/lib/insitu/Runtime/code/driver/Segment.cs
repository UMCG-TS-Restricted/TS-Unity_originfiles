using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		/// <remarks>
		///		Position and Rotation data will always be invalid when using Vicon Nexus.
		/// </remarks>
		public struct Segment
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0x5E6E;

			/// <summary>
			///		Segment parent name
			/// </summary>
			public string parent;

			/// <summary>
			///		Name given to the segment
			/// </summary>
			public string name;

			/// <summary>
			///		The rotation quaternion of the segment in Vicon global transform space.
			///	</summary>
			///	<remarks>
			///		Conversion of the field has not been implemented as it is only data obtained from Vicon Tracker.
			/// </remarks>
			public double4 vicon_rotation;

			/// <summary>
			///		The position of the segment in Vicon global transform space.
			///	</summary>
			public double3 vicon_position;

			/// <summary>
			///		Flag if the rotation value is valid [0: invalid; otherwise: valid]
			///	</summary>
			public byte valid_rotation;

			/// <summary>
			///		Flag if the position value is valid [0: invalid; otherwise: valid]
			///	</summary>
			public byte valid_position;


			/// <summary>
			///		Pose composed of vicon_rotation and vicon_position.
			/// </summary>
			public readonly pose vicon_pose => new pose
			{
				position = vicon_position,
				valid_position = valid_position,
				rotation = vicon_rotation,
				valid_rotation = valid_rotation,
			};

			/// <summary>
			///		Serializes <paramref name="segment"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry telemetry, Segment segment)
			{
				telemetry.Begin(TypeId, Telemetry.FlagBody, 1);
				telemetry.WriteCached(segment.parent);
				telemetry.WriteCached(segment.name);
				telemetry.Write(segment.vicon_rotation);
				telemetry.Write(segment.vicon_position);
				telemetry.Write(segment.valid_rotation);
				telemetry.Write(segment.valid_position);
				telemetry.End();
			}

			/// <summary>
			///		Deserializes from <paramref name="value"/> and stores it in <paramref name="segment"/>.
			///		If deserializing fails it will return false, if successful it will return true.
			/// </summary>
			public static bool Read(telemetry.Object value, array<string> string_cache, out Segment segment)
			{
				segment = default;
				if (value.type != TypeId)
					return false;

				var reader = value.Read(string_cache);
				reader = reader.read_cached(out segment.parent);
				reader = reader.read_cached(out segment.name);
				reader = reader.read(out segment.vicon_rotation);
				reader = reader.read(out segment.vicon_position);
				reader = reader.read(out segment.valid_rotation);
				reader = reader.read(out segment.valid_position);
				return true;
			}

			/// <summary>
			///		Convert to Json.
			/// </summary>
			public readonly Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["header"] = TypeId;
				obj["type"] = "segment";
				obj["parent"] = parent ?? string.Empty;
				obj["name"] = name ?? string.Empty;
				obj["vicon_rotation"] = vicon_rotation.json();
				obj["vicon_position"] = vicon_position.json();
				obj["rotation_valid"] = valid_rotation;
				obj["position_valid"] = valid_position;
				return obj;
			}

			/// <summary>
			///		Convert from Json.
			/// </summary>
			public static Segment FromJson(Json.Object json)
			{
				if (json == null)
					return default;

				var marker = new Segment();
				marker.parent = json["parent"];
				if (marker.parent == null)
					marker.parent = string.Empty;

				marker.name = json["name"];
				if (marker.name == null)
					marker.name = string.Empty;

				marker.vicon_rotation = double4.from(json.ArrayOf("vicon_rotation"), 0, double4.identity);
				marker.vicon_position = double3.from(json.ArrayOf("vicon_position"));
				marker.valid_rotation = json["rotation_valid"];
				marker.valid_position = json["position_valid"];
				return marker;
			}
		}
	}
}
