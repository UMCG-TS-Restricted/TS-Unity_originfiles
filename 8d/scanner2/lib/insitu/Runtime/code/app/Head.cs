using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Uses Vicon's Vusion to combine HMD and Mocap
	/// </summary>
	public class Head : PoseBehaviour
	{
		[NonSerialized] public Vicon.Vusion Vusion;
		[NonSerialized] public Vicon.Worker Worker;

		public PoseBehaviour Device;
		public PoseBehaviour Mocap;


		public void Start()
		{
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
			var pose = new pose();
			if (Device != null)
				pose = Device.Pose();
			return pose;
		}

		public pose ViconPose()
		{
			var pose = new pose();
			if (Mocap != null)
				pose = Mocap.Pose();
			return pose;
		}

		public void OnPreRender()
		{
			Start();

			var hmd = HmdPose();
			var vicon = ViconPose();
			double3 head_position;
			double4 head_rotation;
			if (vicon.valid_position > 0 && vicon.valid_rotation > 0)
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
			if (vusion != null && hmd.valid_rotation > 0)
				head_rotation = vusion.Update(vicon.rotation, vicon.valid_rotation > 0, hmd.rotation);

			transform.localPosition = head_position.v3();
			transform.localRotation = head_rotation.q();
		}

		public static void DrawPose(pose pose, Color color, float fov)
		{
			if (pose.rotation.sqrmagnitude() < 0.01)
				return;

			color.a *= Mathf.Lerp(0.3f, 1.0f, pose.valid_position * pose.valid_rotation);
			Gizmos.color = color;
			Gizmos.matrix = Matrix4x4.TRS(pose.position.v3(), pose.rotation.q(), Vector3.one);
			Gizmos.DrawFrustum(Vector3.zero, fov, 0.8f, 0.01f, 1.0f);
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
