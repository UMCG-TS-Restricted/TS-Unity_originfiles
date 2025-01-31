using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct Subject
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0x50BE;

			/// <summary>
			///		Name of subject
			/// </summary>
			public string name;

			/// <summary>
			///		References the slice of the marker array inside State.
			/// </summary>
			/// <see cref="State.markers"/>
			public range markers;

			/// <summary>
			///		References the slice of the segment array inside State.
			/// </summary>
			/// <see cref="State.segments"/>
			public range segments;

			/// <summary>
			///		Serializes <paramref name="subject"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry telemetry, Subject subject)
			{
				telemetry.Begin(TypeId, Telemetry.FlagBody, 1);
				telemetry.WriteCached(subject.name);
				telemetry.Write(subject.markers.offset);
				telemetry.Write(subject.markers.length);
				telemetry.Write(subject.segments.offset);
				telemetry.Write(subject.segments.length);
				telemetry.End();
			}

			/// <summary>
			///		Deserializes from <paramref name="value"/> and stores it in <paramref name="subject"/>.
			///		If deserializing fails it will return false, if successful it will return true.
			/// </summary>
			public static bool Read(telemetry.Object value, array<string> string_cache, out Subject subject)
			{
				subject = default;
				if (value.type != TypeId)
					return false;

				var reader = value.Read(string_cache);
				reader = reader.read_cached(out subject.name);
				reader = reader.read(out subject.markers.offset);
				reader = reader.read(out subject.markers.length);
				reader = reader.read(out subject.segments.offset);
				reader = reader.read(out subject.segments.length);
				return true;
			}

			/// <summary>
			///		Convert to Json
			/// </summary>
			public Json.Object ToJson(slice<Marker> all_markers, slice<Segment> all_segments)
			{
				var obj = new Json.Object();
				obj["header"] = TypeId;
				obj["type"] = "subject";
				obj["name"] = name ?? string.Empty;

				if (all_markers.length > 0)
				{
					var arr = new Json.Array();
					for (var i = 0; i < all_markers.length; i++)
						arr.Add(all_markers[i].ToJson());
					obj["markers"] = arr;
				}

				if (all_segments.length > 0)
				{
					var arr = new Json.Array();
					for (var i = 0; i < all_segments.length; i++)
						arr.Add(all_segments[i].ToJson());
					obj["segments"] = arr;
				}

				return obj;
			}
		}
	}
}
