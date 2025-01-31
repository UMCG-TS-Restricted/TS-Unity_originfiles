using System;
using System.Collections.Generic;
using ADG;
using UnityEngine;


namespace insitu
{
	public class UnityStateSubject : MonoBehaviour
	{
		[NonSerialized] public List<UnityStateMarker> Markers;
		[NonSerialized] public List<UnityStateSegment> Segments;
		[NonSerialized] public Transform MarkersContainer;
		[NonSerialized] public Transform SegmentsContainer;
		[NonSerialized] public int Index;
		[NonSerialized] public int Version;
		[NonSerialized] public string CachedName;

		public App App;

		public void Start()
		{
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


		public void Update()
		{
			if (!App.FetchState(out var state))
				return;

			if (Markers == null)
				Markers = new List<UnityStateMarker>();

			if (Segments == null)
				Segments = new List<UnityStateSegment>();

			var name = gameObject.name;
			if (state.version != Version || name != CachedName)
			{
				Version = state.version;
				CachedName = name;
				Index = state.SubjectWith(name);
				if (Index < 0)
				{
					// TODO: Message
					Unity.Clear(Markers);
					Unity.Clear(Segments);
					return;
				}

				var subject = state.subjects[Index];

				Unity.Clear(Markers);
				var markers = subject.markers;
				for (var i = 0; i < markers.length; i++)
				{
					var marker = state.markers[markers.offset + i];
					var obj = new GameObject(marker.name);
					var child = obj.AddComponent<UnityStateMarker>();
					child.App = App;
					child.Subject = new StringReference(name);
					obj.transform.SetParent(MarkersContainer, false);
					Markers.Add(child);
				}

				Unity.Clear(Segments);
				var segments = subject.segments;
				for (var i = 0; i < segments.length; i++)
				{
					var segment = state.segments[segments.offset + i];
					if (string.IsNullOrEmpty(segment.parent))
					{
						var obj = new GameObject(segment.name);
						var child = obj.AddComponent<UnityStateSegment>();
						child.App = App;
						child.Subject = new StringReference(name);
						obj.transform.SetParent(SegmentsContainer, false);
						Segments.Add(child);
					}
				}
			}
		}
	}
}
