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
		[NonSerialized] public StateHandle Handle;
		[NonSerialized] public Device Device;
		[NonSerialized] public slice<DeviceOutput> Output;

		[Note("Device reference to Vicon device. The data can only be accessed at runtime as the Output is fully dynamic.")]
		public App App;
		public string Name;

		/// <see cref="State.Of(Device, ref StateHandle, out DeviceOutput)"/>
		public DeviceOutput OutputOf(string name)
		{
			for (var i = 0; i < Output.length; i++)
			{
				var element = Output[i];
				if (string.Equals(element.name, name, StringComparison.Ordinal))
					return element;
			}

			return default;
		}

		/// <see cref="State.Of(Device, ref StateHandle, out DeviceOutput)"/>
		public DeviceOutput OutputOf(string name, ref int version, ref int index)
		{
			if (version != Handle.version)
			{
				version = Handle.version;
				for (var i = 0; i < Output.length; i++)
				{
					var element = Output[i];
					if (string.Equals(element.name, name, StringComparison.Ordinal))
					{
						index = i;
						return element;
					}
				}

				index = -1;
				return default;
			}

			if (index >= 0 && index < Output.length)
				return Output[index];

			return default;
		}

		public bool Scan(State state) => state.Of(ref Handle, out Device);

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);

		public bool Fetch(State state)
		{
			Handle = Handle.Update(Name);
			if (!state.Of(ref Handle, out Device))
				return false;

			var range = Device.outputs;
			var outputs = range.slice(state.outputs.elements);
			Output = outputs;
			return true;
		}

		public void Update() => Fetch();
	}
}