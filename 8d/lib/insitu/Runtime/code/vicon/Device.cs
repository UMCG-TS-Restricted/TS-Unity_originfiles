using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct Device : Telemetry.IObject
		{
			public string name;
			public range outputs;

			public ushort Header => 0xDE1C;
			public byte Version => 1;
			public int Capacity => 128 + 16;


			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, name, 0, 128);
				dst = buffer.write(dst, outputs.index);
				dst = buffer.write(dst, outputs.length);
				return (int)(dst - start);
			}

			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha)
			{
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = buffer.read(ptr, next.swap_endian, out name, out _);
					ptr = buffer.read(ptr, next.swap_endian, out outputs.index);
					ptr = buffer.read(ptr, next.swap_endian, out outputs.length);
				}
			}

			public Json.Object ToJson(State state)
			{
				var obj = new Json.Object();
				obj["name"] = name ?? string.Empty;

				var arr = new Json.Array();
				for (var i = 0; i < outputs.length; i++)
					arr.Add(state.outputs[outputs.index + i].ToJson());
				obj["outputs"] = arr;

				return obj;
			}
		}
	}
}
