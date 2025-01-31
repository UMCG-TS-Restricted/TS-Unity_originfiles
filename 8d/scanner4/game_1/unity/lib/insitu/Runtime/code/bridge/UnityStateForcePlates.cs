using System;
using System.Collections.Generic;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		A collection of UnityStateForcePlate.
	///		This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime.
	///		Use UnityStateForcePlate directly to make references in the editor.
	/// </summary>
	public class UnityStateForcePlates : MonoBehaviour
	{
		[NonSerialized] public int Version;
		[NonSerialized] public List<UnityStateForcePlate> Plates;

		[Note("A collection of UnityStateForcePlate. This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime. Use UnityStateForcePlate directly to make references in the editor.")]
		public App App;

		public void Awake()
		{
			Plates = new List<UnityStateForcePlate>();
		}

		/// <summary>
		///		Rebuild children
		/// </summary>
		public bool Scan(State state)
		{
			Version = state.version;
			Util.Clear(Plates);

			var plates = state.plates;
			for (var i = 0; i < plates.length; i++)
			{
				var plate = plates[i];
				var obj = new GameObject("plate: " + i);
				var child = obj.AddComponent<UnityStateForcePlate>();
				child.enabled = false;
				child.App = App;
				child.Index = i;
				child.Plate = plate;
				obj.transform.SetParent(transform, false);
				Plates.Add(child);
			}

			return true;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);

		public bool Fetch(State state)
		{
			if (state.version != Version)
				Scan(state);

			var plates = Plates;
			for (var i = 0; i < plates.Count; i++)
			{
				var plate = plates[i];
				plate.Fetch(state);
				plate.gameObject.SetActive(plate.CachedIndex >= 0);
			}

			return true;
		}

		public void Update() => Fetch();
	}
}
