using System;
using ADG;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	public class UnityStateDevice : MonoBehaviour
	{
		[NonSerialized] public int Index;
		[NonSerialized] public int Version;
		[NonSerialized] public slice<DeviceOutput> Output;
		[NonSerialized] public string CachedName;

		public App App;

		[TextArea(8, 24)]
		public string Data;


		public void Update()
		{
			if (!App.FetchState(out var state))
				return;

			var name = gameObject.name;
			if (state.version != Version || CachedName != name)
			{
				Version = state.version;
				CachedName = name;
				Index = state.DeviceWith(name);
			}

			if (Index < 0)
			{
				// TODO: Message
				return;
			}

			var device = state.devices[Index];
			var range = device.outputs;
			var outputs = range.slice(state.outputs.elements);
			var array = new Json.Array { };
			for (var i = 0; i < outputs.length; i++)
				array.Add(outputs[i].ToJson());


			Output = outputs;
			Data = array.Stringify(Json.Pretty);
		}
	}
}