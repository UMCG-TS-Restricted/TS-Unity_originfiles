using System;
using ADG;
using System.IO;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace insitu
{
	/// <summary>
	///		Application settings and application state.
	///		This is used like a singleton, but without global state.
	///		To use this, create the object in the project view.
	///		Assign the serialized object to components requiring application state and -settings.
	///		
	///		You are able to inherit this class and add additional data.
	///		This is useful for difficulty settings.
	///		Make sure you use the Settings object instead of fields for settings.
	/// </summary>
	[CreateAssetMenu(fileName = "App", menuName = "insitu/Shared App Data")]
	public class App : ScriptableObject
	{
		/// <summary>
		///		Path to store settings
		/// </summary>
		/// <seealso cref="Save(Json.Object)"/>
		public static string SettingsPath => Path.Combine(Application.dataPath, "settings.json");


		/// <code>
		/// {
		///		host: string = IP address to the Vicon server formatted as: 127.0.0.1.
		///		post: number = Port to the Vicon server formatted as: 801
		///		streaming_mode: number = Streaming mode of Vicon [0: ClientPull, 1: ClientPullPreFetch (default), 2: ServerPush].
		///		vicon_mode: number = Type of data to expect [0: None, 1: Vicon Nexus, 2: Vicon Tracker],
		/// }
		/// </code>
		[NonSerialized] public Json.Object Settings;
		[NonSerialized] public Vicon.Worker Worker;

		/// <summary>
		///		Used to determine telemetry timestamps
		/// </summary>
		[NonSerialized] public Stopwatch Stopwatch;

		/// <summary>
		///		Initializes data.
		///		Call this as soon as possible.
		/// </summary>
		public virtual void Initialize()
		{
			Worker = null;
			Settings = Load(true);
			Stopwatch = Stopwatch.StartNew();
		}

		public double Time() => Stopwatch.Elapsed.TotalSeconds;

		/// <see cref="Save(Json.Object)"/>
		public void Save() => Save(Settings);

		/// <summary>
		///		Writes a Json object to SettingsPath.
		/// </summary>
		/// <seealso cref="SettingsPath"/>
		public static void Save(Json.Object settings) => File.WriteAllText(SettingsPath, settings.Stringify(Json.Pretty));

		/// <summary>
		///		Return settings, if it does not exist load from disk.
		/// </summary>
		public Json.Object FetchSettings() => Settings ??= Load(false);

		/// <summary>
		///		Load settings from disk.
		/// </summary>
		public Json.Object LoadSettings() => Settings = Load(false);

		/// <summary>
		///		Load Json object file from disk.
		/// </summary>
		public static Json.Object Load(bool log, string path)
		{
			if (File.Exists(path))
			{
				var text = File.ReadAllText(path);
				var json = Json.ParseObject(text);
				if (log) Debug.Log($"Loaded {path} with contents: {text}");
				return json;
			}
			else if (log)
				Debug.Log($"{path} not found");

			return null;
		}

		/// <summary>
		///		Loads setting files defined by the developer and user.
		/// </summary>
		/// <remarks>
		///		Load settings from the data path first.
		///		Overwrites the values from the persistent data path afterwards.
		///		The result is never null.
		///	</remarks>
		/// <param name="log">Debug.Log the load results.</param>
		public static Json.Object Load(bool log)
		{
			var result = new Json.Object();
			Load(log, SettingsPath)?.CopyTo(result);
			//Load(log, UserSettingsPath)?.CopyTo(result);
			return result;
		}

		/// <summary>
		///		Checks if the state is valid.
		/// </summary>
		public static bool FetchState(App app)
		{
			if (!app) return false;
			var worker = app.Worker;
			if (worker == null) return false;
			return worker.State.version > 0;
		}

		/// <summary>
		///		Checks if the state is valid and stores it in <paramref name="state"/>.
		/// </summary>
		public static bool FetchState(App app, out Vicon.State state)
		{
			state = default;
			if (!app) return false;
			var worker = app.Worker;
			if (worker == null) return false;
			state = worker.State;
			return state.version > 0;
		}
	}
}
