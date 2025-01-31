using System;
using System.Collections.Generic;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		A collection of UnityStateUnlabeledMarker.
	///		This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime.
	///		Use UnityStateUnlabeledMarker directly to make references in the editor.
	/// </summary>
	public class UnityStateUnlabeledMarkers : MonoBehaviour
	{
		[NonSerialized] public int Version;
		[NonSerialized] public List<UnityStateUnlabeledMarker> Markers;

		[Note("A collection of UnityStateUnlabeledMarker. This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime. Use UnityStateUnlabeledMarker directly to make references in the editor.")]
		public App App;

		public void Awake()
		{
			Markers = new List<UnityStateUnlabeledMarker>();
		}

		public void ApplyCurrent()
		{
			var markers = Markers;
			for (var i = 0; i < markers.Count; i++)
			{
				var marker = markers[i];
				marker.ApplyCurrent();
			}
		}

		/// <summary>
		///		Rebuild children
		/// </summary>
		public bool Scan(State state)
		{
			var markers = state.unlabeled;
			while (Markers.Count < markers.length)
			{
				var obj = new GameObject("unlabeled");
				var child = obj.AddComponent<UnityStateUnlabeledMarker>();
				child.enabled = false;
				child.App = App;
				obj.transform.SetParent(transform, false);
				Markers.Add(child);
			}

			var index = 0;
			while (index < markers.length)
			{
				var marker = markers[index];
				var component = Markers[index];
				component.Index = index;
				component.Version = state.version;
				component.CachedId = marker.id;
				component.Id = marker.id;
				component.Marker = marker;
				component.gameObject.name = $"unlabeled {marker.id}";
				component.gameObject.SetActive(true);
				index++;
			}

			while (index < Markers.Count)
			{
				var component = Markers[index];
				component.gameObject.SetActive(false);
				component.gameObject.name = "(cached unlabeled marker)";
				index++;
			}

			return true;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);

		public bool Fetch(State state) => Scan(state);

		public void Update()
		{
			if (Fetch())
				ApplyCurrent();
		}
	}
}
