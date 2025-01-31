using System;
using System.Collections.Generic;
using UnityEngine;


namespace insitu
{
	public class UnityStateDevices : MonoBehaviour
	{
		[NonSerialized] public int Version;
		[NonSerialized] public List<UnityStateDevice> Devices;

		public App App;

		public void Update()
		{
			if (!App.FetchState(out var state))
				return;

			if (Devices == null)
				Devices = new List<UnityStateDevice>();

			var version = state.version;
			if (version != Version)
			{
				Version = version;

				for (var i = 0; i < Devices.Count; i++)
					Destroy(Devices[i].gameObject);
				Devices.Clear();

				var devices = state.devices;
				for (var i = 0; i < devices.length; i++)
				{
					var device = devices[i];
					var obj = new GameObject(device.name);
					var child = obj.AddComponent<UnityStateDevice>();
					child.App = App;
					obj.transform.SetParent(transform, false);
					Devices.Add(child);
				}
			}
		}
	}
}
