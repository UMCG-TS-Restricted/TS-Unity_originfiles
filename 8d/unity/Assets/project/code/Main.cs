using UnityEngine;
using insitu;
using System;
//using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


public sealed class Main : MonoBehaviour
{
	public App App;
	public ViconSimulator Simulator;


	public void Awake()
	{
		App.Initialize();
	}

	public void Start()
	{
		if (Simulator && Simulator.enabled)
		{
			Simulator.Create();
			App.Worker = Simulator.Worker;
			Simulator.Scan(); // TODO: Add delay for realness
		}
		else
		{
			var settings = App.Settings;
			var host_json = settings.StringOf("host");
			var host = host_json == null
				? "127.0.0.1:801"
				: host_json.Value;

			var mode_json = settings.NumberOf("mode");
			var mode = mode_json == null
				? Vicon.ClientPullPreFetch
				: (int)mode_json.Value;

			var worker = new Vicon.Worker(host, mode);
			if (worker.DLL == IntPtr.Zero)
			{
				Debug.LogError("Failed to create worker");
				return;
			}

			var err = worker.ConfigureWireless(out var message);
			if (err != Vicon.Success)
				Debug.LogWarning(message);

			worker.Start();
			App.Worker = worker;
		}

		App.Telemetry = new Telemetry { };
		App.Telemetry.Register(App.Worker.State);
	}


	public bool __Test;
	public bool __Record;

	public void FixedUpdate()
	{
		if (__Record && App.Telemetry != null)
		{
			var entity = App.Telemetry.Entities[0];
			entity.reference = App.Worker.State;
			App.Telemetry.Entities[0] = entity;
			App.Telemetry.MoveNext(Time.frameCount, Time.fixedTime);
		}
	}

	public void Update()
	{
		var worker = App.Worker;
		if (worker == null)
			return;


		if (__Test)
		{
			__Record = false;
			__Test = false;
			Debug.Log(worker.State);
			App.Telemetry.Save("test.dat");
		}

		var time = worker.Stopwatch.ElapsedMilliseconds;
		var state = worker.StateVersion;
		if (state == 0 && time > 5000)
		{
			Debug.LogError("Failed to start Vicon worker - stopping..");
			OnDisable();
			return;
		}

		var error = worker.Error;
		if (!string.IsNullOrEmpty(error))
		{
			Debug.LogError(error);
			worker.Error = null;
		}


	}

	public void OnDisable()
	{
		if (App.Worker != null)
		{
			App.Worker.Dispose();
			App.Worker = null;
		}
	}

	[ContextMenu("Print state")]
	public void PrintState()
	{
		if (App && App.Worker != null)
		{
			var state = App.Worker.State;
			Debug.Log(state.ToString());
		}
	}

	public void OnDrawGizmos()
	{
		if (App && App.Worker != null)
		{
			var state = App.Worker.State;
			var subjects = state.subjects;
			for (var i = 0; i < subjects.length; i++)
			{
				var subject = subjects[i];
				var marker_slice = subject.markers;
				for (var j = 0; j < marker_slice.length; j++)
				{
					var index = marker_slice.index + j;
					var marker = state.markers[index];
					Gizmos.color = marker.valid != 0
						? new Color(1.0f, 0.4f, 0.1f, 0.8f)
						: new Color(1.0f, 0.6f, 0.2f, 0.2f);
					Gizmos.DrawSphere(marker.position.v3(), 0.06f);
				}
			}
		}
	}
}
