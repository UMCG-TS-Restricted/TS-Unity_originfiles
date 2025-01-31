using System;
using System.Collections.Generic;
using ADG;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	public class UnityStateSegment : PoseBehaviour
	{
		[NonSerialized] public int Index;
		[NonSerialized] public int Version;
		[NonSerialized] public string CachedName;
		[NonSerialized] public string CachedSubject;
		[NonSerialized] public List<UnityStateMarker> Markers;
		[NonSerialized] public List<UnityStateSegment> Segments;
		[NonSerialized] public Segment Segment;

		public App App;
		public StringReference Subject;

		[TextArea(8, 24)]
		public string Data;

		public bool Fetch()
		{
			if (!App.FetchState(out var state))
				return false;

			if (Segments == null)
				Segments = new List<UnityStateSegment>();

			if (Markers == null)
				Markers = new List<UnityStateMarker>();

			var name = gameObject.name;
			var subject_name = Subject.Value;
			if (state.version != Version || name != CachedName || subject_name != CachedSubject)
			{
				Version = state.version;
				CachedName = name;
				CachedSubject = subject_name;
				Unity.Clear(Markers);
				Unity.Clear(Segments);

				var subject_index = state.SubjectWith(subject_name);
				if (subject_index < 0)
				{
					// TODO: Message
					Index = -1;
					return false;
				}

				var subject = state.subjects[subject_index];
				var segment_index = state.SegmentWith(subject.markers, name);
				Index = segment_index;

				var segments = subject.segments;
				for (var i = 0; i < segments.length; i++)
				{
					var segment = state.segments[segments.offset + i];
					if (segment.parent == name)
					{
						var obj = new GameObject(segment.name);
						var child = obj.AddComponent<UnityStateSegment>();
						child.App = App;
						child.Subject = Subject;
						obj.transform.SetParent(transform, false);
						Segments.Add(child);
					}
				}

				var markers = subject.markers;
				for (var i = 0; i < markers.length; i++)
				{
					var marker = state.markers[markers.offset + i];
					if (marker.parent == name)
					{
						var obj = new GameObject(marker.name);
						var child = obj.AddComponent<UnityStateMarker>();
						child.App = App;
						child.Subject = Subject;
						obj.transform.SetParent(transform, false);
						Markers.Add(child);
					}
				}
			}

			if (Index < 0)
				return false;

			Segment = state.segments[Index];
			return true;
		}

		public override pose Pose()
		{
			if (Fetch())
			{
				var segment = Segment;
				return segment.pose;
			}

			return new pose
			{
				position = double3.from(transform.position),
				position_valid = 0,
				rotation = double4.from(transform.rotation),
				rotation_valid = 0,
			};
		}

		public void Update()
		{
			if (Fetch())
			{
				var pose = Segment.pose;
				if (pose.position_valid != 0)
					transform.position = pose.position.v3();
				if (pose.rotation_valid != 0)
					transform.rotation = pose.rotation.q();
				Data = Segment.ToJson().Stringify(Json.Pretty);
			}
		}
	}
}
