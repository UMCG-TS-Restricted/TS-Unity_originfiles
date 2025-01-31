using System;
using insitu;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class TelemetryRigidbody : MonoBehaviour, Telemetry.IEntity, Telemetry.IDeserialize
{
	public const ushort Version = 1;
	public const byte Position = 1 << 1;
	public const byte Rotation = 1 << 2;
	public const byte Velocity = 1 << 3;
	public const byte Angular  = 1 << 4;

	public struct Data
	{
		public const ushort capacity = sizeof(byte) + buffer.sizeof_v3 + buffer.sizeof_v4 + buffer.sizeof_v3 + buffer.sizeof_v3;

		public byte flags;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 velocity;
		public Vector3 angular;

		public ushort length()
		{
			ushort size = sizeof(byte); // flags
			if ((flags & Position) != 0) size += buffer.sizeof_v3;
			if ((flags & Rotation) != 0) size += buffer.sizeof_v4;
			if ((flags & Velocity) != 0) size += buffer.sizeof_v3;
			if ((flags & Angular) != 0) size += buffer.sizeof_v3;
			return size;
		}

		public unsafe ushort write(byte* dst)
		{
			var ptr = dst;
			dst = buffer.write(dst, flags);
			if ((flags & Position) != 0) dst = buffer.write(dst, position);
			if ((flags & Rotation) != 0) dst = buffer.write(dst, rotation);
			if ((flags & Velocity) != 0) dst = buffer.write(dst, velocity);
			if ((flags & Angular) != 0) dst = buffer.write(dst, angular);
			return (ushort)(ptr - dst);
		}

		public unsafe ushort write_full(slice dst)
		{
			fixed (byte* ptr = dst.span)
				return write_full(ptr);
		}

		public unsafe ushort write_full(byte* dst)
		{
			var ptr = dst;
			dst = buffer.write(dst, byte.MaxValue);
			dst = buffer.write(dst, position);
			dst = buffer.write(dst, rotation);
			dst = buffer.write(dst, velocity);
			dst = buffer.write(dst, angular);
			return (ushort)(ptr - dst);
		}

		public static byte flags_of(Data from, Data to)
		{
			byte result = 0;
			if (buffer.Differs(from.position, to.position)) result |= Position;
			if (buffer.Differs(from.rotation, to.rotation)) result |= Rotation;
			if (buffer.Differs(from.velocity, to.velocity)) result |= Velocity;
			if (buffer.Differs(from.angular, to.angular)) result |= Angular;
			return result;
		}


		public static unsafe Data read(slice data, bool endian)
		{
			fixed (byte* ptr = data.span)
				return read(ptr, endian);
		}

		public static unsafe Data read(byte* data, bool endian)
		{
			if (data == null)
			{
				return new Data
				{
					flags = 0,
					rotation = Quaternion.identity,
				};
			}

			var result = new Data { };
			data = buffer.read(data, endian, out result.flags);
			if ((result.flags & Position) != 0) data = buffer.read(data, endian, out result.position);
			if ((result.flags & Rotation) != 0) data = buffer.read(data, endian, out result.rotation);
			if ((result.flags & Velocity) != 0) data = buffer.read(data, endian, out result.velocity);
			if ((result.flags & Angular) != 0) data = buffer.read(data, endian, out result.angular);
			return result;
		}

		public static Data from(Data old, Data cur, byte flags) => new Data
		{
			flags = flags,
			position = (cur.flags & Position) != 0 ? cur.position : old.position,
			rotation = (cur.flags & Rotation) != 0 ? cur.rotation : old.rotation,
			velocity = (cur.flags & Velocity) != 0 ? cur.velocity : old.velocity,
			angular = (cur.flags & Angular) != 0 ? cur.angular : old.angular,
		};

		public static Data from(Rigidbody r, byte flags) => new Data
		{
			flags = flags,
			position = r.position,
			rotation = r.rotation,
			velocity = r.velocity,
			angular = r.angularVelocity,
		};
	}

	public identifier id;

	[NonSerialized] public Rigidbody Rigidbody;
	public Rigidbody RequestRigidbody => Rigidbody ? Rigidbody : (Rigidbody = GetComponent<Rigidbody>());

	ushort Telemetry.IObject.Header => 0xEBD1;
	byte Telemetry.IObject.Version => 1;
	int Telemetry.ICache.Capacity => Data.capacity;
	public int Identifier => id;

	public unsafe int Serialize(slice cache, byte* dst)
	{
		var old = Data.read(cache, false);
		var cur = Data.from(RequestRigidbody, 0);
		cur.flags = Data.flags_of(old, cur);
		cur.flags = byte.MaxValue; // TEST!!
		if (old.flags != 0 && cur.flags == 0)
			return 0;

		cur.write(dst);
		if (cache.length > 0)
		{
			var v = Data.from(old, cur, cur.flags);
			v.write_full(dst);
		}

		return cur.length();
	}

	public void Deserialize(slice current, SerializedBuffer next, int flags, float alpha)
	{
		var a = Data.read(current, false);
		var change = Data.read(next.slice, next.swap_endian);
		var b = Data.from(a, change, change.flags);
		var c = new Data
		{
			flags = byte.MaxValue,
			position = Vector3.LerpUnclamped(a.position, b.position, alpha),
			rotation = buffer.Differs(a.rotation, new Quaternion(0,0,0,0))
				? Quaternion.LerpUnclamped(a.rotation, b.rotation, alpha)
				: b.rotation,
			velocity = Vector3.LerpUnclamped(a.velocity, b.velocity, alpha),
			angular = Vector3.LerpUnclamped(a.angular, b.angular, alpha),
		};

		if (a.flags != 0)
			Debug.DrawLine(a.position, c.position);

		if (flags != 0)
		{
			var r = RequestRigidbody;
			r.position = c.position;
			r.rotation = c.rotation;
		}

		c.write_full(current);
	}

	public bool CanDeserialize(ushort header) => header == 0xEBD1;
}
