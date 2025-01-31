using System;


namespace insitu
{
	[Serializable]
	public class Reference
	{
		public const int None = 0;
		public const int Marker = 1;
		public const int Segment = 2;
		public const int Subject = 3;

		[NonSerialized] public array<double4> Snapshot;
		[NonSerialized] public array<double4> Positions;
		[NonSerialized] public bool QueueSnapshot;

		[NonSerialized] public int PreviousType;
		[NonSerialized] public string PreviousSubject;
		[NonSerialized] public string PreviousSelf;
		[NonSerialized] public int StateVersion;
		[NonSerialized] public int SubjectIndex;
		[NonSerialized] public int SelfIndex;

		public int Type;
		public string SubjectName;
		public string SelfName;

		public void Scan(Vicon.State state, int version)
		{
			var parent_index = -1;
			var index = -1;
			switch (Type)
			{
				case Marker:
				{
					parent_index = state.SubjectWith(SubjectName);
					var subject = state.subjects.At(parent_index);
					index = state.MarkerOf(subject.markers, SelfName);
				} break;

				case Segment:
				{
					parent_index = state.SubjectWith(SubjectName);
					var subject = state.subjects.At(parent_index);
					index = state.SegmentOf(subject.segments, SelfName);

					var markers = subject.markers;
					Snapshot = Snapshot.Reuse(markers.length);
					Positions = Positions.Reuse(markers.length);
					QueueSnapshot = true;
				} break;

				case Subject:
				{
					parent_index = state.SubjectWith(SubjectName);
					index = parent_index;
				} break;
			}

			PreviousType = Type;
			PreviousSelf = SelfName;
			PreviousSubject = SubjectName;
			StateVersion = version;
			SelfIndex = index;
			SubjectIndex = parent_index;
		}

		public pose SubjectPose(Vicon.State state)
		{
			var subject = state.subjects.At(SubjectIndex);
			var markers = subject.markers;
			var positions = Positions;
			var reference = Snapshot;

			var success = 0;
			for (var i = 0; i < markers.length; i++)
			{
				var index = markers.index + i;
				var marker = state.markers[index];
				var w = marker.valid;
				positions[i] = marker.position.d4(w);
				success += w;
			}

			if (QueueSnapshot)
			{
				for (var i = 0; i < markers.length; i++)
					reference[i] = positions[i];

				QueueSnapshot = success < 4;
			}

			var matrix = double4x4.transformed_by(markers.length, reference, positions);
			return new pose
			{
				rotation = matrix.rotation(),
				position = matrix.position(),
				rotation_valid = success > 3 ? 1 : 0,
				position_valid = success > 0 ? 1 : 0,
			};
		}

		public pose Pose(Vicon.State state, int state_version)
		{
			if (state_version == 0) return default;
			if (state_version != StateVersion ||
				Type != PreviousType ||
				SelfName != PreviousSelf ||
				SubjectName != PreviousSubject)
				Scan(state, state_version);

			switch (Type)
			{
				case Marker:
				{
					if (state.markers.At(SelfIndex, out var marker)) return new pose
					{
						position = marker.position,
						position_valid = marker.valid,
					};
				} break;

				case Segment:
				{
					var segment = state.segments.At(SelfIndex);
					var pose = segment.pose;
					if (pose.position_valid == 0 || pose.rotation_valid == 0)
					{

					}
				} break;

				case Subject:
				{

				} break;
			}




			return default;
		}



		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Index(Vicon.State state, int subject)
		{
			if (subject < 0) return -1;
			var value = state.subjects[subject];
			if (Type == Marker) return state.MarkerOf(value.markers, Name);
			if (Type == Segment) return state.SegmentOf(value.segments, Name);
			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Index(Vicon.State state, string subject)
		{
			var index = state.SubjectWith(subject);
			return Index(state, index);
		}*/
	}
}
