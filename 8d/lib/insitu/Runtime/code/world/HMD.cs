using System;
using UnityEngine;
//using UnityEngine.InputSystem.XR;
//using UnityEngine.InputSystem;

namespace insitu
{
	public class HMD : MonoBehaviour
	{
		//[NonSerialized] public XRHMD XRHMD;
		[NonSerialized] public Vicon.Vusion Vusion;
		[NonSerialized] public Vicon.Worker Worker;
		[NonSerialized] public int StateVersion;
		[NonSerialized] public int SubjectIndex;
		[NonSerialized] public int SegmentIndex;
		[NonSerialized] public array<int> Markers;
		[NonSerialized] public array<double4> Reference;
		[NonSerialized] public array<double4> Positions;
		[NonSerialized] public bool QueueReference;


		public App App;
		public string SubjectName;
		public string[] MarkerNames;


		public void Reset()
		{
			App = Unity.FindResource<App>();
			SubjectName = "TestSub2";
			MarkerNames = new string[]
			{
				"HMD1",
				"HMD2",
				"HMD3",
				"HMD4",
				"HMD5",
			};
		}


		public void Start()
		{
			if (App == null)
			{
				Debug.LogError(App.NotAssigned, this);
			}

			/*if (XRHMD == null || XRHMD.enabled == false)
			{
				XRHMD result = null;
				var devices = InputSystem.devices;
				for (var i = 0; i < devices.Count; i++)
				{
					if (devices[i] is XRHMD hmd && hmd.enabled)
					{
						result = hmd;
						break;
					}
				}

				XRHMD = result;
			}*/

			if (Vusion == null)
			{
				var fusion = new Vicon.Vusion();
				if (fusion.DLL == IntPtr.Zero)
				{
					Debug.LogError("Could not start fusion service.");
				}
				else
				{
					Vusion = fusion;
				}
			}
		}

		public void OnDestroy()
		{
			if (Vusion != null)
			{
				Vusion.Dispose();
				Vusion = null;
			}
		}

		public void Update() => OnPreRender();
		public void LateUpdate() => OnPreRender();

		public pose HmdPose()
		{
			//var hmd = XRHMD;
			var pose = new pose();
			/*if (hmd != null)
			{
				var rotation_control = hmd.deviceRotation;
				var rotation = rotation_control.ReadValue();
				var position_control = hmd.devicePosition;
				var position = position_control.ReadValue();
				pose.rotation = double4.from(rotation);
				pose.position = double3.from(position);
				pose.rotation_valid = 0;
				pose.position_valid = 0;
			}*/
			return pose;
		}

		public pose ViconPose()
		{
			var app = App;
			if (!app) return default;
			var worker = app.Worker;
			if (!app) return default;

			var pose = new pose();
			if (worker != null && worker.StateVersion > 0)
			{
				var state = worker.State;
				if (worker.StateVersion != StateVersion)
				{
					var markers = new array<int>(MarkerNames.Length, MarkerNames.Length);
					var segment_index = -1;
					var subject_index = state.SubjectWith(SubjectName);
					if (subject_index >= 0)
					{
						var subject = state.subjects[subject_index];
						for (var i = 0; i < MarkerNames.Length; i++)
						{
							var index = state.MarkerOf(subject.markers, MarkerNames[i]);
							markers[i] = index;
						}
					}

					Reference = Reference.Reuse(markers.length);
					Positions = Positions.Reuse(markers.length);
					QueueReference = markers.length != Markers.length;
					StateVersion = worker.StateVersion;
					SubjectIndex = subject_index;
					SegmentIndex = segment_index;
					Markers = markers;
				}

				if (SegmentIndex >= 0 && app.ViconMode == App.ViconTracker)
				{
					var segment = state.segments[SegmentIndex];
					pose = segment.pose;
				}
				else if (Markers.length > 0)
				{
					var positions = Positions;
					var markers = Markers;
					var success = 0;
					for (var i = 0; i < markers.length; i++)
					{
						var index = markers[i];
						var marker = state.markers[index];
						var w = marker.valid;
						positions[i] = marker.position.d4(w);
						success += w;
					}

					var reference = Reference;
					if (QueueReference)
					{
						for (var i = 0; i < markers.length; i++)
							reference[i] = positions[i];
						QueueReference = success < 4;
					}

					var matrix = double4x4.transformed_by(markers.length, reference, positions);
					pose.rotation = matrix.rotation();
					pose.position = matrix.position();
					pose.rotation_valid = success > 3 ? 1 : 0;
					pose.position_valid = success > 0 ? 1 : 0;
				}
			}

			return pose;
		}

		public void OnPreRender()
		{
			Start();

			var hmd = HmdPose();
			var vicon = ViconPose();
			double3 head_position;
			double4 head_rotation;
			if (vicon.position_valid > 0 && vicon.rotation_valid > 0)
			{
				head_position = vicon.position;
				head_rotation = vicon.rotation;
			}
			else
			{
				head_position = hmd.position;
				head_rotation = hmd.rotation;
			}

			var vusion = Vusion;
			if (vusion != null && hmd.rotation_valid > 0)
				head_rotation = vusion.Update(vicon.rotation, vicon.rotation_valid > 0, hmd.rotation);

			transform.localPosition = head_position.v3();
			transform.localRotation = head_rotation.q();
		}

		public static void DrawPose(pose pose, Color color, float fov)
		{
			if (pose.rotation.sqr_magnitude() < 0.01)
				return;

			color.a *= Mathf.Lerp(0.3f, 1.0f, pose.position_valid * pose.rotation_valid);
			Gizmos.color = color;
			Gizmos.matrix = Matrix4x4.TRS(pose.position.v3(), pose.rotation.q(), Vector3.one);
			Gizmos.DrawFrustum(Vector3.zero, 60, 0.8f, 0.01f, 1.0f);
		}

		public void OnDrawGizmos()
		{
			var hmd = HmdPose();
			var vicon = ViconPose();
			DrawPose(hmd, Color.blue, 59);
			DrawPose(vicon, Color.green, 60);
		}
	}
}
