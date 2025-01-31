using ADG;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		///		State does not contain an array of labeled markers.
		///		This is of the lacking implementation in the SDK:
		///		* No names can be queried, which causes it to have no value.
		///		* Error prone in case of removing labeled markers.
		/// </remarks>
		public struct State
		{
			/// <summary>
			///		Serialization type identifier.
			/// </summary>
			public const ushort TypeId = 0xE5AE;

			/// <summary>
			///		The state structure version.
			///		Every time a change has been registered in the structure, a Scan in performed, which increases the version number.
			///		The value can be used to determine if cached indices should be recalculated.
			///		version = 0: State has not been initialized; data should not be accessed.
			///		version < 0: Worker has been shut down; data should not be accessed.
			///		version > 0: Worker is running and the data can be accessed.
			/// </summary>
			public volatile int version;

			/// <summary>Frame index retrieved from Vicon.</summary>
			public int frame;

			/// <summary>Time in seconds since the worker started.</summary>
			public double time;
			
			/// <summary>Position offset applied before position_transform</summary>
			///	<seealso cref="Worker.Transform(double4x4)"/>
			public double3 position_center;

			///	<seealso cref="Worker.Transform(double4x4)"/>
			public double4x4 position_transform;

			/// <remarks>
			///		Not serialized; the value can be reconstructed by normalizing the rotation part of position_transform.
			///	</remarks>
			///	<seealso cref="Worker.Transform(double4x4)"/>
			public double3x3 vector_transform;

			public array<Device> devices;
			public array<DeviceOutput> outputs;
			public array<ForcePlate> plates;
			public array<Subject> subjects;
			public array<Marker> markers;

			/// <remarks>
			///		When using Vicon Nexus the position and rotation are 0 values.
			/// </remarks>
			public array<Segment> segments;

			/// <summary>
			///		Unlabeled markers.
			/// </summary>
			/// <remarks>
			///		Note that there is little use in there markers:
			///		* The ID will change when missing a frame by the Vicon camera's
			///		* The ordering will change when there is a change in the ulabeled count.
			/// </remarks>
			public array<Unlabeled> unlabeled;

			/// <summary>
			///		Find the index of a device with name: <paramref name="name"/>.
			///		If it fails to find the device, it will return -1.
			/// </summary>
			public readonly int IndexOfDevice(string name)
			{
				for (var i = 0; i < devices.length; i++)
				{
					var device = devices[i];
					if (string.Equals(name, device.name))
						return i;
				}

				return -1;
			}

			/// <summary>
			///		Find the index of a device output with name: <paramref name="name"/>.
			///		If it fails to find the device, it will return -1.
			///		<paramref name="slice"/> can be obtained from the device:
			///		<see cref="Device.outputs"/>
			/// </summary>
			public readonly int IndexOfOutput(range slice, string name)
			{
				var start = slice.offset;
				var end = start + slice.length;
				for (var i = start; i < end; i++)
				{
					var output = outputs[i];
					if (string.Equals(name, output.name))
						return i;
				}

				return -1;
			}

			/// <summary>
			///		Find the index of a subject with name: <paramref name="name"/>.
			///		If it fails to find the subject, it will return -1.
			/// </summary>
			public readonly int IndexOfSubject(string name)
			{
				for (var i = 0; i < subjects.length; i++)
				{
					var subject = subjects[i];
					if (string.Equals(name, subject.name))
						return i;
				}

				return -1;
			}

			/// <summary>
			///		Find the index of a marker with name: <paramref name="name"/>.
			///		If it fails to find the marker, it will return -1.
			///		<paramref name="slice"/> can be obtained from the subject:
			///		<see cref="Subject.markers"/>
			/// </summary>
			public readonly int IndexOfMarker(range slice, string name)
			{
				var start = slice.offset;
				var end = start + slice.length;
				for (var i = start; i < end; i++)
				{
					var marker = markers[i];
					if (string.Equals(name, marker.name))
						return i;
				}

				return -1;
			}

			/// <summary>
			///		Find the index of a segment with name: <paramref name="name"/>.
			///		If it fails to find the segment, it will return -1.
			///		<paramref name="slice"/> can be obtained from the subject:
			///		<see cref="Subject.segments"/>
			/// </summary>
			public readonly int IndexOfSegment(range slice, string name)
			{
				var start = slice.offset;
				var end = start + slice.length;
				for (var i = start; i < end; i++)
				{
					var segment = segments[i];
					if (string.Equals(name, segment.name))
						return i;
				}

				return -1;
			}

			/// <summary>
			///		Find the index of an unlabeled marker with id: <paramref name="id"/>.
			///		If it fails to find the marker, it will return -1.
			/// </summary>
			public readonly int IndexOfUnlabeled(uint id)
			{
				for (var i = 0; i < unlabeled.length; i++)
				{
					var marker = unlabeled[i];
					if (id == marker.id)
						return i;
				}

				return -1;
			}

			/// <summary>
			///		Retrieve current device data by <paramref name="handle"/> name.
			///		The procedure updates the version and index.
			/// </summary>
			/// <returns>
			///		If the device with the name has been found.
			///	</returns>
			public readonly bool Of(ref StateHandle handle, out Device device)
			{
				var current_version = version;
				if (current_version != handle.version)
				{
					handle.version = current_version;
					handle.index = IndexOfDevice(handle.name);
				}

				if (handle.index < 0 || handle.index >= devices.length)
				{
					device = default;
					return false;
				}

				device = devices[handle.index];
				return true;
			}

			/// <summary>
			///		Retrieve current device output data by <paramref name="handle"/> name.
			///		The procedure updates the version and index.
			/// </summary>
			/// <returns>
			///		If the device output with the name has been found.
			///		Make sure to still check if <paramref name="output"/> has valid data.
			///	</returns>
			public readonly bool Of(Device device, ref StateHandle handle, out DeviceOutput output)
			{
				var current_version = version;
				if (current_version != handle.version)
				{
					handle.version = current_version;
					handle.index = IndexOfOutput(device.outputs, handle.name);
				}

				if (handle.index < 0 || handle.index >= devices.length)
				{
					output = default;
					return false;
				}

				output = outputs[handle.index];
				return true;
			}

			/// <summary>
			///		Retrieve current subject data by <paramref name="handle"/> name.
			///		The procedure updates the version and index.
			/// </summary>
			/// <returns>
			///		If the subject with the name has been found.
			///	</returns>
			public readonly bool Of(ref StateHandle handle, out Subject subject)
			{
				var current_version = version;
				if (current_version != handle.version)
				{
					handle.version = current_version;
					handle.index = IndexOfSubject(handle.name);
				}

				if (handle.index < 0 || handle.index >= devices.length)
				{
					subject = default;
					return false;
				}

				subject = subjects[handle.index];
				return true;
			}

			/// <summary>
			///		Retrieve current marker data by <paramref name="handle"/> name.
			///		The procedure updates the version and index.
			/// </summary>
			/// <returns>
			///		If the marker with the name has been found.
			///		Make sure to still check if <paramref name="marker"/> has valid data.
			///		<seealso cref="Marker.valid_position"/>
			///	</returns>
			public readonly bool Of(Subject subject, ref StateHandle handle, out Marker marker)
			{
				var current_version = version;
				if (current_version != handle.version)
				{
					handle.version = current_version;
					handle.index = IndexOfMarker(subject.markers, handle.name);
				}

				if (handle.index < 0 || handle.index >= devices.length)
				{
					marker = default;
					return false;
				}

				marker = markers[handle.index];
				return true;
			}

			/// <summary>
			///		Retrieve current segment data by <paramref name="handle"/> name.
			///		The procedure updates the version and index.
			/// </summary>
			/// <returns>
			///		If the segment with the name has been found.
			///		Make sure to still check if <paramref name="segment"/> has valid data.
			///		<seealso cref="Segment.valid_position"/>
			///		<seealso cref="Segment.valid_rotation"/>
			///	</returns>
			public readonly bool Of(Subject subject, ref StateHandle handle, out Segment segment)
			{
				var current_version = version;
				if (current_version != handle.version)
				{
					handle.version = current_version;
					handle.index = IndexOfSegment(subject.segments, handle.name);
				}

				if (handle.index < 0 || handle.index >= devices.length)
				{
					segment = default;
					return false;
				}

				segment = segments[handle.index];
				return true;
			}

			/// <summary>
			///		Serializes <paramref name="state"/> to <paramref name="writer"/>.
			/// </summary>
			public static void Write(Telemetry writer, State state)
			{
				writer.Begin(TypeId, Telemetry.FlagBody, 1);
				writer.Write(state.version);
				writer.Write(state.frame);
				writer.Write(state.time);
				writer.Write(state.position_center);
				writer.Write(state.position_transform);
				writer.Write(state.devices.length);
				writer.Write(state.outputs.length);
				writer.Write(state.plates.length);
				writer.Write(state.subjects.length);
				writer.Write(state.markers.length);
				writer.Write(state.segments.length);
				writer.Write(state.unlabeled.length);
				writer.End();

				for (var i = 0; i < state.devices.length; i++)
					Device.Write(writer, state.devices[i]);

				for (var i = 0; i < state.outputs.length; i++)
					DeviceOutput.Write(writer, state.outputs[i]);

				for (var i = 0; i < state.plates.length; i++)
					ForcePlate.Write(writer, state.plates[i]);

				for (var i = 0; i < state.subjects.length; i++)
					Subject.Write(writer, state.subjects[i]);

				for (var i = 0; i < state.markers.length; i++)
					Marker.Write(writer, state.markers[i]);

				for (var i = 0; i < state.segments.length; i++)
					Segment.Write(writer, state.segments[i]);

				for (var i = 0; i < state.unlabeled.length; i++)
					Unlabeled.Write(writer, state.unlabeled[i]);
			}

			/// <summary>
			///		Returns the amount of entities read, and thus needed to advance.
			///		0 means nothing has been read, and state is its default value.
			/// </summary>
			public static int Read(array<telemetry.Object> objects, int offset, array<string> string_cache, ref State state)
			{
				state = default;
				var obj = objects[offset];
				if (obj.type != TypeId)
					return 0;

				// Read self
				{
					var reader = obj.Read(string_cache);
					reader = reader.read(out state.version);
					reader = reader.read(out state.frame);
					reader = reader.read(out state.time);
					reader = reader.read(out state.position_center);
					reader = reader.read(out state.position_transform);

					reader = reader.read(out state.devices.length);
					reader = reader.read(out state.outputs.length);
					reader = reader.read(out state.plates.length);
					reader = reader.read(out state.subjects.length);
					reader = reader.read(out state.markers.length);
					reader = reader.read(out state.segments.length);
					reader = reader.read(out state.unlabeled.length);

					state.vector_transform = double3x3.normalized(state.position_transform);
					state.devices = state.devices.Reuse(state.devices.length);
					state.outputs = state.outputs.Reuse(state.outputs.length);
					state.plates = state.plates.Reuse(state.plates.length);
					state.subjects = state.subjects.Reuse(state.subjects.length);
					state.markers = state.markers.Reuse(state.markers.length);
					state.segments = state.segments.Reuse(state.segments.length);
					state.unlabeled = state.unlabeled.Reuse(state.unlabeled.length);
				}


				var count = 1;
				for (var i = 0; i < state.devices.length; i++)
				{
					Device.Read(objects[offset + count + i], string_cache, out var device);
					state.devices[i] = device;
				}
				count += state.devices.length;

				for (var i = 0; i < state.outputs.length; i++)
				{
					DeviceOutput.Read(objects[offset + count + i], string_cache, out var output);
					state.outputs[i] = output;
				}
				count += state.outputs.length;

				for (var i = 0; i < state.plates.length; i++)
				{
					ForcePlate.Read(objects[offset + count + i], state.vector_transform, out var plate);
					state.plates[i] = plate;
				}
				count += state.plates.length;

				for (var i = 0; i < state.subjects.length; i++)
				{
					Subject.Read(objects[offset + count + i], string_cache, out var subject);
					state.subjects[i] = subject;
				}
				count += state.subjects.length;

				for (var i = 0; i < state.markers.length; i++)
				{
					Marker.Read(objects[offset + count + i], string_cache, state.position_center, state.position_transform, out var marker);
					state.markers[i] = marker;
				}
				count += state.markers.length;

				for (var i = 0; i < state.segments.length; i++)
				{
					Segment.Read(objects[offset + count + i], string_cache, out var segment);
					state.segments[i] = segment;
				}
				count += state.segments.length;

				for (var i = 0; i < state.unlabeled.length; i++)
				{
					Unlabeled.Read(objects[offset + count + i], state.position_center, state.position_transform, out var marker);
					state.unlabeled[i] = marker;
				}
				count += state.unlabeled.length;

				return count;
			}

			/// <summary>
			///		Convert to Json.
			/// </summary>
			public Json.Object ToJson(bool full)
			{
				var obj = new Json.Object();
				obj["header"] = TypeId;
				obj["type"] = "state";
				obj["version"] = version;
				obj["frame"] = frame;
				obj["time"] = time;
				obj["position_center"] = position_center.json();
				obj["position_transform"] = position_transform.ToJson();
				obj["vector_transform"] = vector_transform.ToJson();

				if (full)
				{
					Json.Array arr;
					obj["devices"] = arr = new Json.Array();
					for (var i = 0; i < devices.length; i++)
					{
						var device = devices[i];
						var slice = device.outputs.slice(outputs.elements);
						arr.Add(devices[i].ToJson(slice));
					}

					obj["plates"] = arr = new Json.Array();
					for (var i = 0; i < plates.length; i++)
						arr.Add(plates[i].ToJson());

					obj["subjects"] = arr = new Json.Array();
					for (var i = 0; i < subjects.length; i++)
					{
						var subject = subjects[i];
						var marker_slice = subject.markers.slice(markers.elements);
						var segment_slice = subject.segments.slice(segments.elements);
						arr.Add(subjects[i].ToJson(marker_slice, segment_slice));
					}

					obj["unlabeled"] = arr = new Json.Array();
					for (var i = 0; i < unlabeled.length; i++)
						arr.Add(unlabeled[i].ToJson());
				}

				return obj;
			}
		}
	}
}