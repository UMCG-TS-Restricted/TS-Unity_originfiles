using System;
using System.Collections.Generic;
using ADG;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		Subject reference to Vicon subject.
	/// </summary>
	public class UnityStateSubject : MonoBehaviour
	{
		[NonSerialized] public List<UnityStateMarker> Markers;
		[NonSerialized] public List<UnityStateSegment> Segments;
		[NonSerialized] public Transform MarkersContainer;
		[NonSerialized] public Transform SegmentsContainer;
		[NonSerialized] public Subject Subject;
		[NonSerialized] public StateHandle Handle;

		public App App;
		public string Name;

		public void Awake()
		{
			Markers = new List<UnityStateMarker>();
			Segments = new List<UnityStateSegment>();

			{
				var obj = new GameObject("markers");
				var tra = obj.transform;
				tra.SetParent(transform, false);
				MarkersContainer = tra;
			}

			{
				var obj = new GameObject("segments");
				var tra = obj.transform;
				tra.SetParent(transform, false);
				SegmentsContainer = tra;
			}
		}

		public void ApplyCurrent()
		{
			var markers = Markers;
			for (var i = 0; i < markers.Count; i++)
				markers[i].ApplyCurrent();

			var segments = Segments;
			for (var i = 0; i < segments.Count; i++)
				segments[i].ApplyCurrent();
		}

		public bool Scan(State state)
		{
			var name = Name;
			Handle.version = state.version;
			Handle.name = name;
			Handle.index = state.IndexOfSubject(name);
			if (Handle.index < 0)
			{
				Handle.index = -1;
				Util.Clear(Markers);
				Util.Clear(Segments);
				return false;
			}

			var subject = state.subjects[Handle.index];

			Util.Clear(Markers);
			var markers = subject.markers;
			for (var i = 0; i < markers.length; i++)
			{
				var marker = state.markers[markers.offset + i];
				var obj = new GameObject(marker.name);
				var child = obj.AddComponent<UnityStateMarker>();
				child.enabled = false;
				child.App = App;
				child.Subject = new StringReference(name);
				child.Name = marker.name;
				obj.transform.SetParent(MarkersContainer, false);
				Markers.Add(child);
			}

			Util.Clear(Segments);
			var segments = subject.segments;
			for (var i = 0; i < segments.length; i++)
			{
				var segment = state.segments[segments.offset + i];
				if (string.IsNullOrEmpty(segment.parent))
				{
					var obj = new GameObject(segment.name);
					var child = obj.AddComponent<UnityStateSegment>();
					child.enabled = false;
					child.App = App;
					child.Subject = new StringReference(name);
					child.Name = segment.name;
					obj.transform.SetParent(SegmentsContainer, false);
					Segments.Add(child);
				}
			}

			Subject = subject;
			return true;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(App, state);
		public bool Fetch(App app, State state)
		{
			var name = Name;
			if (state.version != Handle.version || name != Handle.name)
				Scan(state);

			var markers = Markers;
			for (var i = 0; i < markers.Count; i++)
				markers[i].Fetch(state);

			var segments = Segments;
			for (var i = 0; i < segments.Count; i++)
				segments[i].Fetch(app, state);

			return Handle.index >= 0;
		}


		public void Update()
		{
			if (Fetch())
				ApplyCurrent();
		}
	}
}
