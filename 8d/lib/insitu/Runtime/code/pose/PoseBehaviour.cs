using UnityEngine;

namespace insitu
{
	public class PoseBehaviour : MonoBehaviour, IPose
	{
		public virtual pose Pose() => new pose
		{
			position = double3.from(transform.position),
			rotation = double4.from(transform.rotation),
			position_valid = 1,
			rotation_valid = 1,
		};
	}
}