using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct DeviceOutput : Telemetry.IObject
		{
			public string name;
			public int unit;


			public ushort Header => 0xDE10;
			public byte Version => 1;
			public int Capacity => 128 + 12;

			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, name, 0, 128);
				dst = buffer.write(dst, unit);
				return (int)(dst - start);
			}

			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha)
			{
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = buffer.read(ptr, next.swap_endian, out name, out _);
					ptr = buffer.read(ptr, next.swap_endian, out unit);
				}
			}

			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["name"] = name ?? string.Empty;
				obj["unit"] = unit;
				return obj;
			}
		}
	}
}
