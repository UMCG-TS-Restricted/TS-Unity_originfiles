using System;
using System.Security;


namespace insitu
{
	public static partial class Vicon
	{
		public static void Fetch(IntPtr dll)
		{
			// Note: This is a heavy call ~70ms.
			// Get the source code and improve performance.
			ViconDLL.Client_GetFrame(dll);
		}

		/// <summary>
		///		Retrieve the full current state from 
		/// </summary>
		/// <remarks>
		///		Make sure to use locking of the owner of <paramref name="dll"/>.
		///		This procedure calls ViconDLL.Client_GetFrame(dll) and takes quite some time to complete.
		///		<paramref name="dll"/> may not be destroyed in this time; which is very likely.
		///		If <paramref name="dll"/> gets destroyed while in the procedure, the software will crash.
		/// </remarks>
		/// <returns>Whether or not the Scan operation is performed due to changes in the vicon structure</returns>
		[SecurityCritical]
		public static unsafe bool Update(IntPtr dll, ref State state, int vicon_mode)
		{
			if (dll == IntPtr.Zero)
				return false;

			{
				Request<int> frame_number_request;
				ViconDLL.Client_GetFrameNumber(dll, (IntPtr)(&frame_number_request));
				state.frame = frame_number_request.value;
			}

			// Detect simple changes
			var changed = false;
			{
				var count = new Request<int>();
				ViconDLL.Client_GetDeviceCount(dll, (IntPtr)(&count));
				changed |= count.value != state.devices.length;
				ViconDLL.Client_GetForcePlateCount(dll, (IntPtr)(&count));
				changed |= count.value != state.plates.length;
				if (changed) Scan(dll, ref state);
			}

			var devices = state.devices;
			for (var i = 0; i < devices.length; i++)
			{
				var device = devices[i];
				var range = device.outputs;
				for (var j = 0; j < range.length; j++)
				{
					Request<double, bool> result;
					var index = range.offset + j;
					var output = state.outputs[index];
					ViconDLL.Client_GetDeviceOutputValue(dll, device.name, output.name, (IntPtr)(&result));
					output.value = result.value0;
					output.valid = result.result == ViconDLL.Success && result.value1 ? (byte)1 : (byte)0;
					state.outputs[index] = output;
				}
			}


			var plates = state.plates;
			for (int i = 0; i < plates.length; i++)
			{
				Request<double3> result;
				var plate = plates[i];
				var index = (uint)i;

				ViconDLL.Client_GetGlobalForceVector(dll, index, (IntPtr)(&result));
				plate.vicon_force = result.value;
				plate.unity_force = state.vector_transform * plate.vicon_force;
				plate.valid_force = result.result == ViconDLL.Success ? (byte)1 : (byte)0;

				ViconDLL.Client_GetGlobalMomentVector(dll, index, (IntPtr)(&result));
				plate.vicon_moment = result.value;
				plate.unity_moment = state.vector_transform * plate.vicon_moment;
				plate.valid_moment = result.result == ViconDLL.Success ? (byte)1 : (byte)0;

				ViconDLL.Client_GetGlobalCentreOfPressure(dll, index, (IntPtr)(&result));
				plate.vicon_cop = result.value;
				plate.unity_cop = state.vector_transform * plate.vicon_cop;
				plate.valid_cop = result.result == ViconDLL.Success ? (byte)1 : (byte)0;

				plates[i] = plate;
			}

			var subjects = state.subjects;
			for (var i = 0; i < subjects.length; i++)
			{
				Request<double4, bool> rotation;
				Request<double3, bool> position;

				var subject = subjects[i];
				var markers = subject.markers;
				var segments = subject.segments;

				for (var j = 0; j < markers.length; j++)
				{
					var index = markers.offset + j;
					var marker = state.markers[index];
					ViconDLL.Client_GetMarkerGlobalTranslation(dll, subject.name, marker.name, (IntPtr)(&position));
					marker.vicon_position = position.value0;
					marker.unity_position = double4x4.mul(state.position_transform, marker.vicon_position, 1.0);
					marker.valid = (position.result == ViconDLL.Success && !position.value1) ? (byte)1 : (byte)0;
					state.markers[index] = marker;
				}

				if (vicon_mode == ViconDLL.ViconTracker)
				{
					for (var j = 0; j < segments.length; j++)
					{
						var index = segments.offset + j;
						var segment = state.segments[index];

						ViconDLL.Client_GetSegmentLocalRotationQuaternion(dll, subject.name, segment.name, (IntPtr)(&rotation));
						segment.vicon_rotation = rotation.value0;
						segment.valid_rotation = rotation.result == ViconDLL.Success && !rotation.value1 && segment.vicon_rotation.sqrmagnitude() > 0.001 ? (byte)1 : (byte)0;

						ViconDLL.Client_GetSegmentLocalTranslation(dll, subject.name, segment.name, (IntPtr)(&position));
						segment.vicon_position = position.value0;
						segment.valid_position = position.result == ViconDLL.Success && !position.value1 ? (byte)1 : (byte)0;

						state.segments[index] = segment;
					}
				}
				else
				{
					// Nexus does not support segment position / rotation
					for (var j = 0; j < segments.length; j++)
					{
						var index = segments.offset + j;
						var segment = state.segments[index];
						segment.valid_rotation = 0;
						segment.valid_position = 0;
						state.segments[index] = segment;
					}
				}
			}

			{
				Request<uint> count;
				ViconDLL.Client_GetUnlabeledMarkerCount(dll, (IntPtr)(&count));
				var unlabeled = state.unlabeled.Reuse((int)count.value);
				for (var i = 0U; i < count.value; i++)
				{
					Request<double3, uint> position;
					ViconDLL.Client_GetUnlabeledMarkerGlobalTranslation(dll, i, (IntPtr)(&position));
					unlabeled[i] = new Unlabeled
					{
						id = position.value1,
						vicon_position = position.value0,
						unity_position = double4x4.mul(state.position_transform, position.value0, 1.0),
					};
				}
				state.unlabeled = unlabeled;
			}

			return changed;
		}
	}
}
