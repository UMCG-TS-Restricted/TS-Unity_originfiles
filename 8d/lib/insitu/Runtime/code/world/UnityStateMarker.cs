using System;
using ADG;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	public class UnityStateMarker : PoseBehaviour
	{
		[NonSerialized] public int Index;
		[NonSerialized] public int Version;
		[NonSerialized] public string CachedName;
		[NonSerialized] public string CachedSubject;
		[NonSerialized] public Marker Marker;

		public App App;
		public StringReference Subject;

		[TextArea(8, 24)]
		public string Data;


		public bool Fetch()
		{
			if (!App.FetchState(out var state))
				return false;

			var name = gameObject.name;
			var subject_name = Subject.Value;
			if (state.version != Version || name != CachedName || subject_name != CachedSubject)
			{
				Version = state.version;
				CachedName = name;
				CachedSubject = subject_name;

				var subject_index = state.SubjectWith(subject_name);
				if (subject_index < 0)
				{
					// TODO: Message
					Index = -1;
					return false;
				}

				var subject = state.subjects[subject_index];
				var marker_index = state.MarkerWith(subject.markers, name);
				Index = marker_index;
			}

			if (Index < 0)
				return false;

			Marker = state.markers[Index];
			return true;
		}

		public override pose Pose()
		{
			if (Fetch())
			{
				var marker = Marker;
				return marker.pose;
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
				transform.position = Marker.position.v3();
				Data = Marker.ToJson().Stringify(Json.Pretty);
			}
		}
	}
}
