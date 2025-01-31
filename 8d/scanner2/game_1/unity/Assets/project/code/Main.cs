using System;
using System.Collections;
using UnityEngine;
using insitu;


public class Main : MonoBehaviour
{
	public const int RecordNone = 0;
	public const int RecordStart = 1;
	public const int RecordRunning = 2;
	public const int RecordStop = 3;

	public const int StateCallibrate = 0;
	public const int StateMenu = 1;
	public const int StateGame = 2;
	public static bool RunThreaded = true;

	[NonSerialized] public int RecordingState;
	[NonSerialized] public int CurrentState;
	[NonSerialized] public int StateHandle;
	[NonSerialized] public bool QueueScoreCheck;
	[NonSerialized] public int LastBlockIndex;

	public App App;
	public ViconSimulator Simulator;
	public Callibrate Callibrate;
	public Menu Menu;
	public Game Game;


	public void Awake()
	{
		App.Initialize();
	}

	[ContextMenu("Queue Rescan")]
	public void QueueRescan()
	{
		var app = App;
		if (!app) return;
		var worker = app.Worker;
		if (worker == null) return;
		worker.QueueRescan = true;
	}

	public void Start()
	{
		var procedure = Simulator && Simulator.enabled
			? ConnectSimulator()
			: Connect(true);

		StartCoroutine(procedure);
		Callibrate.Initialize(this);
	}

	public IEnumerator Connect(bool safely)
	{
		__start:
		var settings = App.Settings;
		string host = settings.EnsuredStringOf("host", "127.0.0.1");
		int port = settings.EnsuredNumberOf("port", 801);

		if (safely)
		{
			var ping = insitu.Unity.Ping(host, port);
			while (!ping.IsCompleted)
				yield return null;

			var result = ping.Result;
			if (result != null)
			{
				Debug.LogError($"Connection safety test failed at {host}:{port}: {result}");
				yield return new WaitForSecondsRealtime(5);
				goto __start;
			}
		}

		Debug.Log($"Connecting to: {host}:{port}");
		yield return null;
		var mode_json = settings.NumberOf("mode");
		var mode = mode_json == null
				? ViconDLL.ClientPullPreFetch
				: (int)mode_json.Value;

		var worker = new Vicon.Worker($"{host}:{port}", mode, ViconDLL.ViconNexus);
		if (worker.DLL == IntPtr.Zero)
		{
			Debug.LogError("Failed to create worker");
			yield return new WaitForSecondsRealtime(5);
			goto __start;
		}

		var err = worker.ConfigureWireless(out var message);
		if (err != ViconDLL.Success)
			Debug.LogWarning(message);

		worker.OnState = OnState;
		if (RunThreaded)
		{
			worker.Start();
		}
		else
		{
			Vicon.Worker.InternalConnect(worker, worker.DLL);
			Vicon.Worker.InternalSetup(worker, worker.DLL);
		}
		App.Worker = worker;
	}

	public IEnumerator ConnectSimulator()
	{
		for (var i = 0; i < 10; i++)
			yield return null;

		Simulator.Create();
		Simulator.Worker.OnState = OnState;

		for (var i = 0; i < 10; i++)
			yield return null;

		App.Worker = Simulator.Worker;

		for (var i = 0; i < 15; i++)
			yield return null;

		Simulator.Scan();
	}

	/// <remarks>Called on a different thread</remarks>
	public void OnState(Vicon.State state)
	{
		if (RecordingState == RecordRunning)
		{
			var app = App;
			if (!app) return;
			var telemetry = app.Telemetry;
			if (telemetry == null) return;

			if (telemetry.Current.frames != null)
			{
				lock (this)
				{
					Vicon.State.Write(telemetry, state);
				}
			}
		}
	}

	public void StartGame()
	{
		CurrentState = StateGame;
		Game.Initialize(this);
	}

	public void FixedUpdate()
	{
		var telemetry = App.Telemetry;
		if (RecordingState == RecordStart)
		{
			if (telemetry == null)
			{
				telemetry = new Telemetry();
				App.Telemetry = telemetry;
				telemetry.Initialize();
			}
			else
			{
				telemetry.Clear();
			}
			RecordingState = RecordRunning;
		}

		if (RecordingState == RecordStop)
		{
			RecordingState = RecordNone;
		}

		if (telemetry != null && RecordingState == RecordRunning)
		{
			lock (this)
			{
				telemetry.NewFrame(Time.frameCount, Time.time);
				var dirty = telemetry.BlockIndex != LastBlockIndex;
				if (dirty)
				{
					if (App.FetchState(App, out var state))
						Vicon.State.Write(telemetry, state);
				}

				var hitters = App.Hitters;
				if (hitters != null)
				{
					for (var i = 0; i < hitters.Count; i++)
					{
						var hitter = hitters[i];
						if (hitter)
							Hitter.Write(telemetry, hitter);
					}
				}

				var projectiles = Game.Projectiles;
				if (projectiles != null)
				{
					for (var i = 0; i < projectiles.Count; i++)
					{
						var projectile = projectiles[i];
						if (!projectile)
							continue;

						if (projectile.State.id == 0 || dirty)
							Projectile.Write(telemetry, projectile);
					}
				}

				LastBlockIndex = telemetry.BlockIndex;
			}
		}
	}


	public void Update()
	{
		UpdateVicon();

		var deltaTime = Time.deltaTime;
		if (CurrentState == StateCallibrate)
			Callibrate.UpdateActive(this, deltaTime);
		else Callibrate.UpdateInactive(deltaTime, true);

		if (CurrentState == StateMenu)
			Menu.UpdateActive(this, deltaTime);
		else Menu.UpdateInactive(deltaTime);

		if (CurrentState == StateGame)
			Game.UpdateActive(this, deltaTime);
		else Game.UpdateInactive(deltaTime);
	}

	public void UpdateVicon()
	{
		var app = App;
		var worker = app.Worker;
		if (worker == null)
			return;

		if (!RunThreaded)
			Vicon.Worker.InternalUpdate(worker, worker.DLL, worker.ViconMode);

		var state = worker.State;
		var time = worker.Stopwatch.ElapsedMilliseconds;
		if (state.version == 0 && time > 5000)
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
			//state.Draw(new Color(1.0f, 0.4f, 0.1f, 0.8f));
		}
	}
}
