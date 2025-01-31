using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace insitu.xr
{
	public class HMD : PoseBehaviour
	{
		[NonSerialized] public XRHMD XRHMD;

		public bool UsePosition;
		public bool UseRotation;

		public override pose Pose()
		{
			var pose = new pose();
			var xrhmd = XRHMD;

			if (XRHMD == null || XRHMD.enabled == false)
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

				XRHMD = xrhmd = result;
			}

			if (xrhmd != null && XRHMD.enabled)
			{
				if (UsePosition)
				{
					var position_control = xrhmd.devicePosition;
					var position = position_control.ReadValue();
					pose.position = double3.from(position);
					pose.valid_position = 1;
				}

				if (UseRotation)
				{
					var rotation_control = xrhmd.deviceRotation;
					var rotation = rotation_control.ReadValue();
					pose.rotation = double4.from(rotation);
					pose.valid_rotation = 1;
				}
			}

			return pose;
		}
	}
}

