using UnityEngine;

namespace insitu
{
	/// <summary>
	///		A core component of bridging Vicon and Unity data.
	///		Because it is a component, the object can be inherited and chained.
	/// </summary>
	/// <example>
	/// public class MultiplyPositionBy2 : PoseBehaviour
	/// {
	///		public PoseBehaviour Input;
	///		public override pose Pose()
	///		{
	///			var pose = Input.Pose();
	///			pose.position *= 2.0;
	///			return pose;
	///		}
	/// }
	/// </example>
	/// <seealso cref="IPoseSource"/>
	public class PoseBehaviour : MonoBehaviour, IPose
	{
		public virtual pose Pose() => new pose
		{
			position = double3.from(transform.position),
			rotation = double4.from(transform.rotation),
			valid_position = 1,
			valid_rotation = 1,
		};

		/// <summary>
		///		Checks if <paramref name="p"/> is exists and returns its pose.
		///		If it does not exist, the procedure will return pose.identity.
		/// </summary>
		/// <seealso cref="pose.identity"/>
		public static pose Try(PoseBehaviour p) => p ? p.Pose() : pose.identity;
	}
}