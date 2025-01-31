using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		public struct State : Telemetry.IEntity, Telemetry.IGroup
		{
			public array<Device> devices;
			public array<DeviceOutput> outputs;
			public array<ForcePlate> plates;
			public array<Subject> subjects;
			public array<Marker> markers;
			public array<Segment> segments;
			public array<Unlabeled> unlabeled;
			public array<Marker> labeled; // TODO: Maybe needs to be removed due to bad vicon implementation


			public int DeviceWith(string name)
			{
				for (var i = 0; i < devices.length; i++)
				{
					var device = devices[i];
					if (string.Equals(name, device.name))
						return i;
				}

				return -1;
			}


			public int SubjectWith(string name)
			{
				for (var i = 0; i < subjects.length; i++)
				{
					var subject = subjects[i];
					if (string.Equals(name, subject.name))
						return i;
				}

				return -1;
			}

			public int MarkerOf(range slice, string name)
			{
				var start = slice.index;
				var end = start + slice.length;
				for (var i = start; i < end; i++)
				{
					var marker = markers[i];
					if (string.Equals(name, marker.name))
						return i;
				}

				return -1;
			}

			public int SegmentOf(range slice, string name)
			{
				var start = slice.index;
				var end = start + slice.length;
				for (var i = start; i < end; i++)
				{
					var segment = segments[i];
					if (string.Equals(name, segment.name))
						return i;
				}

				return -1;
			}

			
			public ushort Header => 0xE5AE;

			public byte Version => 1;

			public int Capacity => 4096;

			public int Identifier => 1;

			public ushort ChildCount => (ushort)(devices.length + outputs.length + plates.length + subjects.length + markers.length + segments.length + unlabeled.length + labeled.length);


			public Telemetry.IObject ChildAt(int index)
			{
				if (index < devices.length)
					return devices[index];
				index -= devices.length;

				if (index < outputs.length)
					return outputs[index];
				index -= outputs.length;

				if (index < plates.length)
					return plates[index];
				index -= plates.length;

				if (index < subjects.length)
					return subjects[index];
				index -= subjects.length;

				if (index < markers.length)
					return markers[index];
				index -= markers.length;

				if (index < segments.length)
					return segments[index];
				index -= segments.length;

				if (index < unlabeled.length)
					return unlabeled[index];
				index -= unlabeled.length;

				if (index < labeled.length)
					return labeled[index];

				return null;
			}

			public unsafe int Serialize(slice cache, byte* dst)
			{
				var start = dst;
				dst = buffer.write(dst, (ushort)devices.length);
				dst = buffer.write(dst, (ushort)outputs.length);
				dst = buffer.write(dst, (ushort)plates.length);
				dst = buffer.write(dst, (ushort)subjects.length);
				dst = buffer.write(dst, (ushort)markers.length);
				dst = buffer.write(dst, (ushort)segments.length);
				dst = buffer.write(dst, (ushort)unlabeled.length);
				dst = buffer.write(dst, (ushort)labeled.length);
				return (int)(dst - start);
			}

			public static unsafe byte* Deserialize<T>(ref array<T> array, byte* src, bool swap_endian)
			{
				src = buffer.read(src, swap_endian, out ushort length);
				array.Grow(length);
				array.length = length;
				return src;
			}

			public static unsafe State Deserialize(SerializedBuffer next)
			{
				var result = new State { };
				fixed (byte* src = next.slice.span)
				{
					var ptr = src;
					ptr = Deserialize(ref result.devices, ptr, next.swap_endian);
					ptr = Deserialize(ref result.outputs, ptr, next.swap_endian);
					ptr = Deserialize(ref result.plates, ptr, next.swap_endian);
					ptr = Deserialize(ref result.subjects, ptr, next.swap_endian);
					ptr = Deserialize(ref result.markers, ptr, next.swap_endian);
					ptr = Deserialize(ref result.segments, ptr, next.swap_endian);
					ptr = Deserialize(ref result.unlabeled, ptr, next.swap_endian);
					ptr = Deserialize(ref result.labeled, ptr, next.swap_endian);
				}
				return result;
			}


			public Json.Object ToJson()
			{
				Json.Array arr;
				var obj = new Json.Object();
				arr = new Json.Array();
				for (var i = 0; i < devices.length; i++)
					arr.Add(devices[i].ToJson(this));
				obj["devices"] = arr;

				arr = new Json.Array();
				for (var i = 0; i < plates.length; i++)
					arr.Add(plates[i].ToJson());
				obj["plates"] = arr;

				arr = new Json.Array();
				for (var i = 0; i < subjects.length; i++)
					arr.Add(subjects[i].ToJson(this));
				obj["subjects"] = arr;

				arr = new Json.Array();
				for (var i = 0; i < unlabeled.length; i++)
					arr.Add(unlabeled[i].ToJson());
				obj["unlabeled"] = arr;

				arr = new Json.Array();
				for (var i = 0; i < labeled.length; i++)
					arr.Add(labeled[i].ToJson());
				obj["labeled"] = arr;

				return obj;
			}

			public override string ToString() => ToJson().Stringify(Json.Pretty);
		}
	}
}