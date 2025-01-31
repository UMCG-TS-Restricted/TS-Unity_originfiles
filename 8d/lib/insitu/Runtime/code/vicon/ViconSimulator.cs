using System;
using System.Diagnostics;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	public class ViconSimulator : MonoBehaviour
	{
		[NonSerialized] public int StateVersion;
		[NonSerialized] public Worker Worker;

		public ViconSimulatorSubject[] Subjects;


		public void Create()
		{
			Worker = new Worker(
				IntPtr.Zero,
				Stopwatch.StartNew(),
				new Vicon.Version { major = 999, minor = 999, point = 999 },
				"Vicon Simulator",
				ClientPullPreFetch);
		}

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
					segments[segment_slice.index + j] = new Segment
					{
						name = model_segment.gameObject.name,
					};
				}
				var marker_slice = subject.markers;
				for (var j = 0; j < marker_slice.length; j++)
				{
					var model_marker = model.Markers[j];
					markers[marker_slice.index + j] = new Marker
					{
						name = model_marker.gameObject.name,
					};
				}
			}

			var state = Worker.State;
			state.devices = new array<Device>();
			state.outputs = new array<DeviceOutput>();
			state.plates = new array<Vicon.ForcePlate>();
			state.subjects = subjects;
			state.segments = segments;
			state.markers = markers;
			Worker.State = state;
			Worker.StateVersion++;
			StateVersion = Worker.StateVersion;
		}

		public bool Apply(bool rescan)
		{
			if (Worker == null)
				return false;

			if (rescan)
				Scan();

			if (Worker.StateVersion != StateVersion)
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
					var index = data.segments.index + j;
					var segment = state.segments[index];
					var sim_segment = sim_segments[index];
					segment.pose = new pose
					{
						rotation = double4.from(sim_segment.rotation),
						position = double3.from(sim_segment.position),
						rotation_valid = sim_segment.gameObject.activeSelf ? 1 : 0,
						position_valid = sim_segment.gameObject.activeSelf ? 1 : 0,
					};
					state.segments[index] = segment;
				}

				for (var j = 0; j < data.markers.length; j++)
				{
					var index = data.markers.index + j;
					var marker = state.markers[index];
					var sim_marker = sim_markers[index];
					marker.valid = sim_marker.gameObject.activeSelf ? (byte)1 : (byte)0;
					marker.position = double3.from(sim_marker.position);
					state.markers[index] = marker;
				}
			}

			return true;
		}

		public void Update()
		{
			Apply(false);
		}
	}
}
