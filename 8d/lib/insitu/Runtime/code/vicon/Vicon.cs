using System;
using System.Runtime.CompilerServices;
using System.Security;


namespace insitu
{
	/// <summary>
	///		Contains all Vicon specific data.
	/// </summary>
	/// <remarks>
	///		Make sure shared pointer data inside the Vicon struct is registered using:
	///		GCHandle.Alloc(_pointer_type_, GCHandleType.Pinned).
	/// </remarks>
	public static partial class Vicon
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double4 ToUnityQuaternion(double4 v) => new double4
		{
			x = v.y,
			y = -v.z,
			z = -v.x,
			w = v.w,
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double3 ToUnityVector(double3 v) => new double3
		{
			x = 0.01 * -v.y,
			y = 0.01 * v.z,
			z = 0.01 * v.x,
		};

		/// <summary>
		///		Query devices, force plates, subjects, segments and markers.
		/// 	
		/// </summary>
		/// <param name="vicon"></param>
		/// <returns></returns>
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
				Client_GetDeviceCount(dll, (IntPtr)(&count));
				devices = devices.Reuse(count.value);

				// Retrieve devices
				var output_length = 0;
				for (var i = 0; i < devices.length; i++)
				{
					var type = 0;
					var index = (uint)i;
					var device = new Device();
					Client_GetDeviceName(dll, index, byte64.size, (IntPtr)(&buffer), ref type);
					device.name = buffer.ToString();

					Client_GetDeviceOutputCount(dll, device.name, (IntPtr)(&count));
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
						Client_GetDeviceOutputName(dll, device.name, output_index, byte64.size, (IntPtr)(&buffer), ref unit);
						output.name = buffer.ToString();
						output.unit = unit;
						outputs[slice.index + j] = output;
					}
				}
			}

			var plates = state.plates;
			{
				Client_GetForcePlateCount(dll, (IntPtr)(&count));
				plates = plates.Reuse(count.value);
			}

			var subjects = state.subjects;
			var segments = state.segments;
			var markers = state.markers;
			{
				Client_GetSubjectCount(dll, (IntPtr)(&count));
				subjects = subjects.Reuse(count.value);

				var segment_length = 0;
				var marker_length = 0;
				for (var i = 0; i < subjects.length; i++)
				{
					var subject = new Subject();
					var index = (uint)i;
					Client_GetSubjectName(dll, index, byte64.size, (IntPtr)(&buffer));
					subject.name = buffer.ToString();

					Client_GetSegmentCount(dll, subject.name, (IntPtr)(&count));
					subject.segments = new range(segment_length, count.value);
					segment_length += count.value;

					Client_GetMarkerCount(dll, subject.name, (IntPtr)(&count));
					subject.markers = new range(marker_length, count.value);
					marker_length += count.value;

					subjects[i] = subject;
				}

				segments = segments.Reuse(count.value);
				markers = markers.Reuse(count.value);
				for (var i = 0; i < subjects.length; i++)
				{
					var subject = subjects[i];
					var segment_slice = subject.segments;
					for (var j = 0; j < segment_slice.length; j++)
					{
						var segment = new Segment();
						Client_GetSegmentName(dll, subject.name, (uint)j, byte64.size, (IntPtr)(&buffer));
						segment.name = buffer.ToString();
						segments[segment_slice.index + j] = segment;
					}

					var marker_slice = subject.markers;
					for (var j = 0; j < marker_slice.length; j++)
					{
						var marker = new Marker();
						Client_GetMarkerName(dll, subject.name, (uint)j, byte64.size, (IntPtr)(&buffer));
						marker.name = buffer.ToString();
						markers[marker_slice.index + j] = marker;
					}
				}
			}

			// Unlabeled needs to be updated every frame, reset to be consistent
			var unlabeled = state.unlabeled;
			{
				unlabeled.length = 0;
			}

			// Labeled markers
			// TODO: Is it possible to do Client_GetMarkerName with a null name?
			var labeled = state.labeled;
			{
				Client_GetLabeledMarkerCount(dll, (IntPtr)(&count));
				labeled = labeled.Reuse(count.value);
				for (var i = 0U; i < count.value; i++)
				{
					var marker = new Marker();
					//Client_GetMarkerName(dll, null, i, byte64.size, (IntPtr)(&buffer));
					marker.name = string.Empty;// buffer.ToString();
					labeled[i] = marker;
				}
			}

			state = new State
			{
				devices = devices,
				outputs = outputs,
				plates = plates,
				subjects = subjects,
				markers = markers,
				segments = segments,
				unlabeled = unlabeled,
				labeled = labeled,
			};
		}

		/// <summary>
		///		Retrieve the full current state from 
		/// </summary>
		/// <returns>Whether or not the Scan operation is performed due to changes in the vicon structure</returns>
		[SecurityCritical]
		public static unsafe bool Update(IntPtr dll, ref State state, out int frame)
		{
			if (dll == IntPtr.Zero)
			{
				frame = -1;
				return false;
			}

			Client_GetFrame(dll);

			int frame_number;
			{
				Request<int> frame_number_request;
				Client_GetFrameNumber(dll, (IntPtr)(&frame_number_request));
				frame_number = frame_number_request.value;
			}

			// Detect simple changes
			var changed = false;
			{
				var count = new Request<int>();
				Client_GetDeviceCount(dll, (IntPtr)(&count));
				changed |= count.value != state.devices.length;
				Client_GetForcePlateCount(dll, (IntPtr)(&count));
				changed |= count.value != state.plates.length;
				Client_GetLabeledMarkerCount(dll, (IntPtr)(&count));
				changed |= count.value != state.labeled.length;
				if (changed)
					Scan(dll, ref state);
			}

			var plates = state.plates;
			for (int i = 0; i < plates.length; i++)
			{
				Request<double3> result;
				var plate = plates[i];
				var index = (uint)i;

				// TODO: Do we need to fix the force results?
				Client_GetGlobalForceVector(dll, index, (IntPtr)(&result));
				plate.force = result.value;

				Client_GetGlobalMomentVector(dll, index, (IntPtr)(&result));
				plate.moment = result.value;

				Client_GetGlobalCentreOfPressure(dll, index, (IntPtr)(&result));
				plate.cop = result.value;

				plates[i] = plate;
			}

			// Actors, TODO: Occlusion
			var subjects = state.subjects;
			for (var i = 0; i < subjects.length; i++)
			{
				var subject = subjects[i];
				var markers = subject.markers;
				var segments = subject.segments;

				Request<double4, bool> rotation;
				Request<double3, bool> position;

				for (var j = 0; j < segments.length; j++)
				{
					var index = segments.index + j;
					var segment = state.segments[index];

					Client_GetSegmentLocalRotationQuaternion(dll, subject.name, segment.name, (IntPtr)(&rotation));
					segment.pose.rotation = ToUnityQuaternion(rotation.value0);
					segment.pose.rotation_valid = rotation.result == Success && !rotation.value1 && segment.pose.rotation.sqr_magnitude() > 0.001 ? 1 : 0;

					Client_GetSegmentLocalTranslation(dll, subject.name, segment.name, (IntPtr)(&position));
					segment.pose.position = ToUnityVector(position.value0);
					segment.pose.position_valid = position.result == Success && !position.value1 ? 1 : 0;

					state.segments[index] = segment;
				}

				for (var j = 0; j < markers.length; j++)
				{
					var index = markers.index + j;
					var marker = state.markers[index];
					Client_GetMarkerGlobalTranslation(dll, subject.name, marker.name, (IntPtr)(&position));
					marker.position = ToUnityVector(position.value0);
					marker.valid = (position.result == Success && !position.value1) ? (byte)1 : (byte)0;
					state.markers[index] = marker;
				}
			}

			var unlabeled = state.unlabeled;
			{
				Request<uint> count;
				Client_GetUnlabeledMarkerCount(dll, (IntPtr)(&count));
				unlabeled = unlabeled.Reuse((int)count.value);
				for (var i = 0U; i < count.value; i++)
				{
					Request<double3, uint> position;
					Client_GetUnlabeledMarkerGlobalTranslation(dll, i, (IntPtr)(&position));
					unlabeled[i] = new Unlabeled
					{
						id = position.value1,
						position = position.value0,
					};
				}
			}

			// TODO: What if we remove a label
			var labeled = state.labeled;
			{
				for (var i = 0U; i < labeled.length; i++)
				{
					var marker = state.labeled[i];
					Request<double3, uint> position;
					Client_GetLabeledMarkerGlobalTranslation(dll, i, (IntPtr)(&position));
					marker.position = ToUnityVector(position.value0);
					state.labeled[i] = marker;
				}
			}

			state.unlabeled = unlabeled;
			frame = frame_number;
			return changed;
		}
	}
}
