using System.Xml.Linq;
using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct ForcePlate : Telemetry.IObject
		{
			public double3 force;
			public double3 moment;
			public double3 cop;
			public byte valid;

			public ushort Header => 0xF07A;
			public byte Version => 1;
			public int Capacity => 73;

			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, force);
				dst = buffer.write(dst, moment);
				dst = buffer.write(dst, cop);
				dst = buffer.write(dst, valid);
				return (int)(dst - start);
			}

			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha)
			{
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = buffer.read(ptr, next.swap_endian, out force);
					ptr = buffer.read(ptr, next.swap_endian, out moment);
					ptr = buffer.read(ptr, next.swap_endian, out cop);
					ptr = buffer.read(ptr, next.swap_endian, out valid);
				}
			}

			public Json.Object ToJson()
			{
				var obj = new Json.Object();
				obj["force"] = force.ToJson();
				obj["moment"] = moment.ToJson();
				obj["cop"] = cop.ToJson();
				obj["valid"] = valid;
				return obj;
			}
		}
	}
}
