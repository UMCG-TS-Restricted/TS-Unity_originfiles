using System;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		Device reference to Vicon force plate.
	/// </summary>
	public class UnityStateForcePlate : MonoBehaviour
	{
		[NonSerialized] public int CachedIndex;
		[NonSerialized] public ForcePlate Plate;

		public App App;
		public int Index;

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);

		public bool Fetch(State state)
		{
			var plates = state.plates;
			var index = Index;
			if (index < 0 || index >= plates.length)
			{
				CachedIndex = -1;
				return false;
			}

			CachedIndex = index;
			var plate = plates[index];
			Plate = plate;
			return true;
		}


		public void Update() => Fetch();
	}
}