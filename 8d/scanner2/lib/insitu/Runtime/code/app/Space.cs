using System.Collections;
using ADG;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Calibrate the space and determines the Vicon transform space to
	///		Unity transform space transformation matrix.
	/// </summary>
	public class Space : MonoBehaviour
	{
		public bool QueueSnapshot;
		public bool SaveShapshot;
		public App App;
		public PoseBehaviour Left;
		public PoseBehaviour Center;
		public PoseBehaviour Back;

		public IEnumerator Start()
		{
			while (!App.FetchState(App))
				yield return null;

			LoadSnapshot();
		}

		[ContextMenu("Load Snapshot")]
		public void LoadSnapshotAction() => LoadSnapshot();

		/// <summary>
		///		Load the created transformation matrix from disk, and if successful, apply it to the worker.
		/// </summary>
		public bool LoadSnapshot()
		{
			while (!App.FetchState(App))
				return false;

			var settings = App.FetchSettings();
			var space = settings.ArrayOf("space");
			var scale = settings.NumberOf("scale", 1.0);
			if (double4x4.from(space, out var matrix))
			{
				var worker = App.Worker;
				matrix = double4x4.scale3x3(matrix, scale);
				worker.Transform(matrix, true);
				return true;
			}

			return false;
		}

		/// <summary>
		///		Delete the created transformation from disk.
		/// </summary>
		public void DeleteSnapshot()
		{
			var settings = App.FetchSettings();
			if (settings.Remove("space"))
				App.Save(settings);
		}

		public void Update()
		{
			if (!QueueSnapshot)
				return;

			if (!App.FetchState(App))
			{
				Debug.LogWarning($"Failed to initialze space: Invalid State. Make sure App is assigned, and the Vicon worker is running.", this);
				return;
			}
			if (!Left)
			{
				Debug.LogError($"Failed to initialze space: Pose {Left} not initialized.", this);
				return;
			}
			if (!Center)
			{
				Debug.LogError($"Failed to initialze space: Pose {Center} not initialized.", this);
				return;
			}
			if (!Back)
			{
				Debug.LogError($"Failed to initialze space: Pose {Back} not initialized.", this);
				return;
			}

			Snap(SaveShapshot);
			QueueSnapshot = false;
		}

		/// <summary>
		///		Create a snapshot and perform the calibration.
		/// </summary>
		/// <param name="save">
		///		Save it to disk to be reused.
		/// </param>
		public void Snap(bool save)
		{
			var p0_pose = Left is IPoseSource left
				? left.PoseSource() 
				: Left.Pose();
			if (p0_pose.valid_position == 0)
			{
				Debug.LogWarning($"Failed to initialize space: Marker {Left} has an invalid position.");
				return;
			}

			var p1_pose = Center is IPoseSource center
				? center.PoseSource()
				: Center.Pose();
			if (p1_pose.valid_position == 0)
			{
				Debug.LogWarning($"Failed to initialize space: Marker {Center} has an invalid position.");
				return;
			}

			var p2_pose = Back is IPoseSource back
				? back.PoseSource()
				: Back.Pose();
			if (p2_pose.valid_position == 0)
			{
				Debug.LogWarning($"Failed to initialize space: Marker {Back} has an invalid position.");
				return;
			}

			var p0 = p0_pose.position;
			var p1 = p1_pose.position;
			var p2 = p2_pose.position;
			var x = double3.normalize(p1 - p0);
			var z = double3.normalize(p1 - p2);
			var y = double3.normalize(double3.cross(x, z));
			var unity_to_vicon = double3x3.from(x, y, z);
			var vicon_to_unity = double3x3.inverse(unity_to_vicon);

			var settings = App.Settings;

			if (App.Worker != null)
			{
				var input = vicon_to_unity;
				if (settings != null)
				{
					double scale = settings.NumberOf("scale", 1.0);
					input = double3x3.scale(input, scale);
				}

				var matrix = double4x4.from(input, default);
				//var matrix = double4x4.from(input, -p1);
				App.Worker.Transform(matrix, true);
			}

			if (save)
			{
				if (settings == null)
				{
					Debug.LogError("Failed to save the space: App is not initialized.");
				}
				else
				{
					//var matrix = double4x4.from(vicon_to_unity, -p1);
					var matrix = double4x4.from(vicon_to_unity, default);
					var json = matrix.ToJson();
					settings["space"] = json;
					App.Save(settings);
					Debug.Log("Space successfully saved!\n" + json.Stringify(Json.Pretty));
				}
			}
		}
	}
}
