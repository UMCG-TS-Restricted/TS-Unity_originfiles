using System;
using System.Security;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		///		Query devices, force plates, subjects, segments and markers.
		/// </summary>
		[SecurityCritical]
		public static unsafe void Scan(IntPtr dll, ref State state)
		{
			if (dll == IntPtr.Zero)
				return;

			var buffer = new byte64();
			var count = new Request<int>();
			var devices = state.devices;
			var outputs = state.outputs;
			{
				ViconDLL.Client_GetDeviceCount(dll, (IntPtr)(&count));
				devices = devices.Reuse(count.value);

				// Retrieve devices
				var output_length = 0;
				for (var i = 0; i < devices.length; i++)
				{
					var type = 0;
					var index = (uint)i;
					var device = new Device();
					ViconDLL.Client_GetDeviceName(dll, index, byte64.size, (IntPtr)(&buffer), ref type);
					device.name = buffer.ToString();

					ViconDLL.Client_GetDeviceOutputCount(dll, device.name, (IntPtr)(&count));
					device.outputs = new range(output_length, count.value);
					output_length += count.value;
					devices[i] = device;
				}

				// Retrieve device outputs
				outputs = outputs.Reuse(output_length);
				for (var i = 0; i < devices.length; i++)
				{
					var device = devices[i];
					var slice = device.outputs;
					for (var j = 0; j < slice.length; j++)
					{
						var output = new DeviceOutput();
						var output_index = (uint)j;
						var unit = 0;
						ViconDLL.Client_GetDeviceOutputName(dll, device.name, output_index, byte64.size, (IntPtr)(&buffer), ref unit);
						output.name = buffer.ToString();
						output.unit = unit;
						outputs[slice.offset + j] = output;
					}
				}
			}

			var plates = state.plates;
			{
				ViconDLL.Client_GetForcePlateCount(dll, (IntPtr)(&count));
				plates = plates.Reuse(count.value);
			}

			var subjects = state.subjects;
			var segments = state.segments;
			var markers = state.markers;
			{
				ViconDLL.Client_GetSubjectCount(dll, (IntPtr)(&count));
				subjects = subjects.Reuse(count.value);

				var segment_length = 0;
				var marker_length = 0;
				for (var i = 0; i < subjects.length; i++)
				{
					var subject = new Subject();
					var index = (uint)i;
					ViconDLL.Client_GetSubjectName(dll, index, byte64.size, (IntPtr)(&buffer));
					subject.name = buffer.ToString();

					ViconDLL.Client_GetSegmentCount(dll, subject.name, (IntPtr)(&count));
					subject.segments = new range(segment_length, count.value);
					segment_length += count.value;

					ViconDLL.Client_GetMarkerCount(dll, subject.name, (IntPtr)(&count));
					subject.markers = new range(marker_length, count.value);
					marker_length += count.value;

					subjects[i] = subject;
				}

				segments = segments.Reuse(segment_length);
				markers = markers.Reuse(marker_length);
				for (var i = 0; i < subjects.length; i++)
				{
					var subject = subjects[i];

					var marker_slice = subject.markers;
					for (var j = 0; j < marker_slice.length; j++)
					{
						var marker = new Marker();
						ViconDLL.Client_GetMarkerName(dll, subject.name, (uint)j, byte64.size, (IntPtr)(&buffer));
						marker.name = buffer.ToString();
						ViconDLL.Client_GetMarkerParentName(dll, subject.name, marker.name, byte64.size, (IntPtr)(&buffer));
						marker.parent = buffer.ToString();
						markers[marker_slice.offset + j] = marker;
					}

					var segment_slice = subject.segments;
					for (var j = 0; j < segment_slice.length; j++)
					{
						var segment = new Segment();
						ViconDLL.Client_GetSegmentName(dll, subject.name, (uint)j, byte64.size, (IntPtr)(&buffer));
						segment.name = buffer.ToString();
						ViconDLL.Client_GetSegmentParentName(dll, subject.name, segment.name, byte64.size, (IntPtr)(&buffer));
						segment.parent = buffer.ToString();
						segments[segment_slice.offset + j] = segment;
					}
				}
			}

			state.version++;
			state.devices = devices;
			state.outputs = outputs;
			state.plates = plates;
			state.subjects = subjects;
			state.markers = markers;
			state.segments = segments;
			state.unlabeled.length = 0;
		}
	}
}
