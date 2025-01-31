using System;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		Device reference to Vicon device.
	///		The data can only be accessed at runtime as the Output is fully dynamic.
	/// </summary>
	public class UnityStateDevice : MonoBehaviour
	{
		[NonSerialized] public int Index;
		[NonSerialized] public int Version;
		[NonSerialized] public Device Device;
		[NonSerialized] public slice<DeviceOutput> Output;
		[NonSerialized] public string CachedName;

		[Note("Device reference to Vicon device. The data can only be accessed at runtime as the Output is fully dynamic.")]
		public App App;
		public string Name;

		public void ApplyCurrent() { }

		public bool Scan(State state)
		{
			Version = state.version;
			CachedName = name;
			Index = state.DeviceWith(name);
			return Index >= 0;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);

		public bool Fetch(State state)
		{
			if (state.version != Version || CachedName != Name)
				Scan(state);

			if (Index < 0)
				return false;

			var device = state.devices[Index];
			var range = device.outputs;
			var outputs = range.slice(state.outputs.elements);
			Output = outputs;
			Device = device;
			return true;
		}


		public void Update()
		{
			if (Fetch())
				ApplyCurrent();
		}
	}
}