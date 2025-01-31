using System;
using ADG;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		Marker reference to Vicon labeled marker.
	/// </summary>
	public class UnityStateMarker : PoseBehaviour, IPoseSource
	{
		[NonSerialized] public int Index;
		[NonSerialized] public int Version;
		[NonSerialized] public string CachedName;
		[NonSerialized] public string CachedSubject;
		[NonSerialized] public Marker Marker;

		public App App;
		public StringReference Subject;
		public string Name;

		public void ApplyCurrent() => transform.position = Marker.unity_position.v3();

		public bool Scan(State state)
		{
			var name = Name;
			var subject_name = Subject.Value;

			Version = state.version;
			CachedName = name;
			CachedSubject = subject_name;

			var subject_index = state.SubjectWith(subject_name);
			if (subject_index < 0)
			{
				Index = -2;
				return false;
			}

			var subject = state.subjects[subject_index];
			var marker_index = state.MarkerWith(subject.markers, name);
			if (marker_index < 0)
			{
				Index = -1;
				return false;
			}

			Index = marker_index;
			return true;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);
		public bool Fetch(State state)
		{
			var name = Name;
			var subject_name = Subject.Value;
			if (state.version != Version || name != CachedName || subject_name != CachedSubject)
				Scan(state);

			if (Index < 0)
				return false;

			Marker = state.markers[Index];
			return true;
		}

		public void Update()
		{
			if (Fetch())
				ApplyCurrent();
		}

		public override pose Pose()
		{
			if (!enabled)
				return Marker.unity_pose;

			if (Fetch())
				return Marker.unity_pose;

			return new pose
			{
				position = double3.from(transform.position),
				valid_position = 0,
				rotation = double4.from(transform.rotation),
				valid_rotation = 0,
			};
		}

		public void OnDrawGizmos()
		{
			var pose = Pose();
			if (pose.valid_position == 0)
				return;

			var position = pose.position.v3();
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(position, 0.1f);
		}

		public pose PoseSource()
		{
			if (!enabled)
				return Marker.vicon_pose;

			if (Fetch())
				return Marker.vicon_pose;

			return pose.identity;
		}
	}
}
