using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct Unlabeled : Telemetry.IObject
		{
			public uint id;
			public double3 position;

			public ushort Header => 0x06AB;
			public byte Version => 1;
			public int Capacity => 128 + 16;

			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, id);
				dst = buffer.write(dst, position);
				return (int)(dst - start);
			}

			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha)
			{
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = buffer.read(ptr, next.swap_endian, out id);
					ptr = buffer.read(ptr, next.swap_endian, out position);
				}
			}

			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["id"] = id;
				obj["position"] = position.ToJson();
				return obj;
			}

			public override string ToString() => ToJson().ToString();
		}
	}
}
