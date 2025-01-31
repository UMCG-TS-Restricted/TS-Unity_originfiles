using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct Segment : Telemetry.IObject
		{
			// TODO: Detect if we allocate memory of the name string when a call to a DLL happens - if so, convert to byte*
			public string name;
			public pose pose;

			public ushort Header => 0x5E6E;
			public byte Version => 1;
			public int Capacity => 128 + 64 + 8;

			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, name ?? string.Empty, 0, 128);
				dst = buffer.write(dst, pose.rotation);
				dst = buffer.write(dst, pose.position);
				dst = buffer.write(dst, pose.rotation_valid);
				dst = buffer.write(dst, pose.position_valid);
				return (int)(dst - start);
			}

			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha)
			{
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = buffer.read(ptr, next.swap_endian, out name, out _);
					ptr = buffer.read(ptr, next.swap_endian, out pose.rotation);
					ptr = buffer.read(ptr, next.swap_endian, out pose.position);
					ptr = buffer.read(ptr, next.swap_endian, out pose.rotation_valid);
					ptr = buffer.read(ptr, next.swap_endian, out pose.position_valid);
				}
			}

			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["name"] = name ?? string.Empty;
				obj["position"] = pose.position.ToJson();
				obj["rotation"] = pose.rotation.ToJson();
				obj["position_valid"] = pose.position_valid;
				obj["rotation_valid"] = pose.rotation_valid;
				return obj;
			}

			public override string ToString() => ToJson().ToString();
		}
	}
}
