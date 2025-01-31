using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		/// Nexus:
		/// 
		/// 
		/// Tracker:
		/// 
		/// 
		/// </summary>
		public struct Subject : Telemetry.IObject
		{
			public string name; // TODO: Should this be byte64 for speed?
			public range markers;
			public range segments;

			public ushort Header => 0x50BE;
			public byte Version => 1;
			public int Capacity => 128 + 24;

			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, name, 0, 128);
				dst = buffer.write(dst, markers.index);
				dst = buffer.write(dst, markers.length);
				dst = buffer.write(dst, segments.index);
				dst = buffer.write(dst, segments.length);
				return (int)(dst - start);
			}

			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha)
			{
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = buffer.read(ptr, next.swap_endian, out name, out _);
					ptr = buffer.read(ptr, next.swap_endian, out markers.index);
					ptr = buffer.read(ptr, next.swap_endian, out markers.length);
					ptr = buffer.read(ptr, next.swap_endian, out segments.index);
					ptr = buffer.read(ptr, next.swap_endian, out segments.length);
				}
			}

			public Json.Object ToJson(State state)
			{
				Json.Array arr;
				var obj = new Json.Object();
				obj["name"] = name ?? string.Empty;

				arr = new Json.Array();
				for (var i = 0; i < markers.length; i++)
					arr.Add(state.markers[markers.index + i].ToJson());
				obj["markers"] = arr;

				arr = new Json.Array();
				for (var i = 0; i < segments.length; i++)
					arr.Add(state.segments[segments.index + i].ToJson());
				obj["segments"] = arr;

				return obj;
			}
		}
	}
}
