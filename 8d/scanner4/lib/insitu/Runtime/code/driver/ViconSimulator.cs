using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		Hijacks the Worker and uses mock subjects instead.
	/// </summary>
	public class ViconSimulator : MonoBehaviour
	{
		[NonSerialized] public int StateVersion;
		[NonSerialized] public Worker Worker;

		public ViconSimulatorSubject[] Subjects;


		public IEnumerator SlowStartup(Action<State> on_state, Action<Worker> on_worker, int wait0 = 10, int wait1 = 10, int wait2 = 20)
		{
			for (var i = 0; i < wait0; i++)
				yield return null;

			Create();
			Worker.OnState = on_state;

			for (var i = 0; i < wait1; i++)
				yield return null;

			on_worker(Worker);

			for (var i = 0; i < wait2; i++)
				yield return null;

			Scan();
		}

		public void Create()
		{
			Worker = new Worker(
				IntPtr.Zero,
				Stopwatch.StartNew(),
				new Vicon.Version { major = 999, minor = 999, point = 999 },
				"Vicon Simulator",
				ViconDLL.ClientPullPreFetch,
				ViconDLL.ViconNexus);

			Worker.Transform(double4x4.identity, default, false);
		}

		/// <see cref="Vicon.Scan(IntPtr, ref State)"/>
		public void Scan()
		{
			if (Worker == null)
				return;

			var simulator_subjects = Subjects;
			var subject_length = simulator_subjects.Length;
			var subjects = new array<Subject>(subject_length, subject_length);
			var segment_length = 0;
			var marker_length = 0;
			for (var i = 0; i < subject_length; i++)
			{
				var model = simulator_subjects[i];
				var subject = new Subject
				{
					name = model.Name,
					segments = new range(segment_length, model.Segments.Length),
					markers = new range(marker_length, model.Markers.Length),
				};
				segment_length += model.Segments.Length;
				marker_length += model.Markers.Length;
				subjects[i] = subject;
				model.Subject = subject;
			}

			var segments = new array<Segment>(segment_length, segment_length);
			var markers = new array<Marker>(marker_length, marker_length);
			for (var i = 0; i < subject_length; i++)
			{
				var model = simulator_subjects[i];
				var subject = model.Subject;
				var segment_slice = subject.segments;
				for (var j = 0; j < segment_slice.length; j++)
				{
					var model_segment = model.Segments[j];
					segments[segment_slice.offset + j] = new Segment
					{
						name = model_segment.gameObject.name,
					};
				}
				var marker_slice = subject.markers;
				for (var j = 0; j < marker_slice.length; j++)
				{
					var model_marker = model.Markers[j];
					markers[marker_slice.offset + j] = new Marker
					{
						name = model_marker.gameObject.name,
					};
				}
			}

			var state = Worker.State;
			state.version = Worker.State.version + 1;
			state.devices = new array<Device>();
			state.outputs = new array<DeviceOutput>();
			state.plates = new array<Vicon.ForcePlate>();
			state.subjects = subjects;
			state.segments = segments;
			state.markers = markers;
			Worker.State = state;
			StateVersion = state.version;
		}

		/// <see cref="Vicon.Update(IntPtr, ref State, int)"/>
		public bool Apply(bool rescan)
		{
			if (Worker == null)
				return false;

			if (Worker.QueueRescan)
			{
				Worker.QueueRescan = false;
				rescan = true;
			}

			if (Worker.QueueTransform)
			{
				Worker.QueueTransform = false;
				var transform = Worker.QueueTransformArg0;
				var rotation = double3x3.normalized(transform);
				Worker.State.position_transform = transform;
				Worker.State.vector_transform = rotation;
			}


			if (rescan)
				Scan();

			if (Worker.State.version != StateVersion)
				return rescan || Apply(true);
		
			var state = Worker.State;
			var simulator_subjects = Subjects;
			if (simulator_subjects.Length != state.subjects.length)
				return rescan || Apply(true);

			for (var i = 0; i < simulator_subjects.Length; i++)
			{
				var subject = simulator_subjects[i];
				var data = subject.Subject;
				var sim_segments = subject.Segments;
				var sim_markers = subject.Markers;
				if (sim_segments.Length != data.segments.length ||
					sim_markers.Length != data.markers.length)
					return rescan || Apply(true);

				for (var j = 0; j < data.segments.length; j++)
				{
					var index = data.segments.offset + j;
					var segment = state.segments[index];
					var sim_segment = sim_segments[j];
					segment.vicon_rotation = double4.from(sim_segment.rotation);
					segment.valid_rotation = sim_segment.gameObject.activeSelf ? (byte)1 : (byte)0;
					segment.vicon_position = double3.from(sim_segment.position);
					segment.valid_position = sim_segment.gameObject.activeSelf ? (byte)1 : (byte)0;
					state.segments[index] = segment;
				}

				for (var j = 0; j < data.markers.length; j++)
				{
					var index = data.markers.offset + j;
					var marker = state.markers[index];
					var sim_marker = sim_markers[j];
					marker.valid_position = sim_marker.gameObject.activeSelf ? (byte)1 : (byte)0;
					marker.vicon_position = double3.from(sim_marker.position);
					marker.unity_position = marker.vicon_position;
					marker.unity_position = marker.unity_position - state.position_center;
					marker.unity_position = double4x4.mul(state.position_transform, marker.unity_position, 1.0);
					state.markers[index] = marker;
				}
			}

			if (Worker.OnState != null)
				Worker.OnState(state);

			return true;
		}

		public void Update()
		{
			Apply(false);
		}
	}
}
