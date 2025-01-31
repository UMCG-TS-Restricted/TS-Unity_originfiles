using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct Marker : Telemetry.IObject
		{
			// TODO: Detect if we allocate memory when a call to a DLL happens - if so, convert to byte*
			public string name;
			public double3 position;
			public byte valid;

			public ushort Header => 0xA4E4;
			public byte Version => 1;
			public int Capacity => 128 + 35;

			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, name, 0, 128);
				dst = buffer.write(dst, position);
				dst = buffer.write(dst, valid);
				return (int)(dst - start);
			}

			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha)
			{
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = buffer.read(ptr, next.swap_endian, out name, out _);
					ptr = buffer.read(ptr, next.swap_endian, out position);
					ptr = buffer.read(ptr, next.swap_endian, out valid);
				}
			}

			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["name"] = name ?? string.Empty;
				obj["position"] = position.ToJson();
				obj["valid"] = valid;
				return obj;
			}

			public override string ToString() => ToJson().ToString();
		}
	}
}
