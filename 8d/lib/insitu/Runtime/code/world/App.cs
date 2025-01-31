using System;
using ADG;
using System.IO;
using UnityEngine;

namespace insitu
{
	/// <summary>
	///		Application settings and application state.
	///		This is used like a singleton, but without global state.
	///		To use this, create the object in the project view.
	///		Assign the serialized object to components requiring application state and -settings.
	/// </summary>
	[CreateAssetMenu(fileName = "App", menuName = "insitu/Shared App Data")]
	public class App : ScriptableObject
	{
		public const int ViconNexus = 1;
		public const int ViconTracker = 2;

		public static readonly string NotAssigned = "App is not assigned: assignment of App is necessary to retrieve Vicon data, without this the object does not work properly.";
		public static string EditorSettingsPath => Path.Combine(Application.dataPath, "settings.json");
		public static string UserSettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");


		/// <code>
		/// {
		///		host: string = IP address to the Vicon server formatted as 127.0.0.1:801.
		///		mode: number = Streaming mode of Vicon [0: ClientPull, 1: ClientPullPreFetch (default), 2: ServerPush].
		/// }
		/// </code>
		[NonSerialized] public Json.Object Settings;
		[NonSerialized] public Vicon.Worker Worker;
		[NonSerialized] public Telemetry Telemetry;
		[NonSerialized] public int ViconMode;


		public void Initialize() => Settings = Load(true);
		public void Save() => File.WriteAllText(UserSettingsPath, Settings.ToString());


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
			Load(log, EditorSettingsPath)?.CopyTo(result);
			Load(log, UserSettingsPath)?.CopyTo(result);
			return result;
		}
	}
}