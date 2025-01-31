using System;
using System.Collections.Generic;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		A collection of UnityStateDevice.
	///		This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime.
	///		Use UnityStateDevice directly to make references in the editor.
	/// </summary>
	public class UnityStateDevices : MonoBehaviour
	{
		[NonSerialized] public int Version;
		[NonSerialized] public List<UnityStateDevice> Devices;

		[Note("A collection of UnityStateDevice. This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime. Use UnityStateDevice directly to make references in the editor.")]
		public App App;

		public void Awake()
		{
			Devices = new List<UnityStateDevice>();
		}

		/// <summary>
		///		Rebuilds children.
		/// </summary>
		public bool Scan(State state)
		{
			Version = state.version;

			for (var i = 0; i < Devices.Count; i++)
				Destroy(Devices[i].gameObject);

			Devices.Clear();

			var devices = state.devices;
			for (var i = 0; i < devices.length; i++)
			{
				var device = devices[i];
				var obj = new GameObject(device.name);
				var child = obj.AddComponent<UnityStateDevice>();
				child.enabled = false;
				child.App = App;
				child.Name = device.name;
				obj.transform.SetParent(transform, false);
				Devices.Add(child);
			}

			return true;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);

		public bool Fetch(State state)
		{
			if (state.version != Version)
				Scan(state);

			return true;
		}

		public void Update() => Fetch();
	}
}
