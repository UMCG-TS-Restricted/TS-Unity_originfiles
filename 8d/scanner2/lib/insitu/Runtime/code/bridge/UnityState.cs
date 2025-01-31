using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Converts the Vicon state to Unity state.
	///		This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime.
	///		Use UnityStateMarker or UnityStateSegment directly to make references in the editor.
	/// </summary>
	public class UnityState : MonoBehaviour
	{
		[NonSerialized] public UnityStateDevices Devices;
		[NonSerialized] public UnityStateForcePlates Plates;
		[NonSerialized] public UnityStateSubjects Subjects;
		[NonSerialized] public UnityStateUnlabeledMarkers Markers;
		[NonSerialized] public Vicon.State State;

		[Note("Converts the Vicon state to Unity state. This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime. Use UnityStateMarker or UnityStateSegment directly to make references in the editor.")]
		public App App;
		
		/// <summary>
		///		Creates child folders
		/// </summary>
		public void Awake()
		{
			// Devices
			{
				var obj = new GameObject("devices");
				var child = obj.AddComponent<UnityStateDevices>();
				child.enabled = false;
				child.App = App;
				obj.transform.SetParent(transform, false);
				Devices = child;
			}

			// Subjects
			{
				var obj = new GameObject("force_plates");
				var child = obj.AddComponent<UnityStateForcePlates>();
				child.enabled = false;
				child.App = App;
				obj.transform.SetParent(transform, false);
				Plates = child;
			}

			// Subjects
			{
				var obj = new GameObject("subjects");
				var child = obj.AddComponent<UnityStateSubjects>();
				child.enabled = false;
				child.App = App;
				obj.transform.SetParent(transform, false);
				Subjects = child;
			}

			// Subjects
			{
				var obj = new GameObject("unlabeled");
				var child = obj.AddComponent<UnityStateUnlabeledMarkers>();
				child.enabled = false;
				child.App = App;
				obj.transform.SetParent(transform, false);
				Markers = child;
			}
		}

		public void Fetch(Vicon.State state)
		{
			State = state;
			gameObject.name = "state v" + state.version + " - frame: " + state.frame;

			if (!Devices)
				Devices = GetComponentInChildren<UnityStateDevices>();

			if (!Plates)
				Plates = GetComponentInChildren<UnityStateForcePlates>();

			if (!Subjects)
				Subjects = GetComponentInChildren<UnityStateSubjects>();

			if (!Markers)
				Markers = GetComponentInChildren<UnityStateUnlabeledMarkers>();

			if (Devices) Devices.Fetch(state);
			if (Plates) Plates.Fetch(state);
			if (Subjects) Subjects.Fetch(App, state);
			if (Markers) Markers.Fetch(state);

			if (Devices) Devices.ApplyCurrent();
			if (Plates) Plates.ApplyCurrent();
			if (Subjects) Subjects.ApplyCurrent();
			if (Markers) Markers.ApplyCurrent();
		}

		/// <summary>
		///		Fetches data from children and applies it afterwards.
		/// </summary>
		public void Update()
		{
			if (App.FetchState(App, out var state))
				Fetch(state);
		}
	}
}
