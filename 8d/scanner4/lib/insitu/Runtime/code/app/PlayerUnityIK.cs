using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Uses Unity IK to animate an actor based on Vicon Bones.
	/// </summary>
	/// <seealso cref="UnityEngine.Animator"/>
	public class PlayerUnityIK : MonoBehaviour
	{
		public App App;
		public Animator Animator;
		public float HipHeight = 1.6f;

		[Header("Bones")]
		public PoseBehaviour LeftFoot;
		public PoseBehaviour RightFoot;
		public PoseBehaviour LeftHand;
		public PoseBehaviour RightHand;
		public PoseBehaviour LeftKnee;
		public PoseBehaviour RightKnee;
		public PoseBehaviour LeftElbow;
		public PoseBehaviour RightElbow;
		public PoseBehaviour LeftHip;
		public PoseBehaviour RightHip;


		public void OnAnimatorIK(int layerIndex)
		{
			var left_hip = PoseBehaviour.Try(LeftFoot);
			var right_hip = PoseBehaviour.Try(RightHip);
			if (left_hip.valid_position != 0 && left_hip.valid_position != 0)
			{
				var left_hip_position = left_hip.position.v3();
				var right_hip_position = right_hip.position.v3();
				var right = (right_hip_position - left_hip_position).normalized;
				var forward = Vector3.Cross(right, Vector3.up);
				transform.forward = forward;
				transform.position = (left_hip_position + right_hip_position) / 2 + new Vector3(0, HipHeight, 0);
			}

			UpdateIK(LeftHand, AvatarIKGoal.LeftHand);
			UpdateIK(RightHand, AvatarIKGoal.RightHand);
			UpdateIK(LeftFoot, AvatarIKGoal.LeftFoot);
			UpdateIK(RightFoot, AvatarIKGoal.RightFoot);
			UpdateHintIK(LeftKnee, AvatarIKHint.LeftKnee);
			UpdateHintIK(RightKnee, AvatarIKHint.RightKnee);
			UpdateHintIK(LeftElbow, AvatarIKHint.LeftElbow);
			UpdateHintIK(RightElbow, AvatarIKHint.RightElbow);
		}

		public void UpdateIK(PoseBehaviour marker, AvatarIKGoal goal)
		{
			var pose = PoseBehaviour.Try(marker);
			if (pose.valid_position == 0)
				return;

			var position = pose.position.v3();
			var animator = Animator;
			animator.SetIKPositionWeight(goal, 1);
			animator.SetIKPosition(goal, position);
		}

		public void UpdateHintIK(PoseBehaviour marker, AvatarIKHint goal)
		{
			var pose = PoseBehaviour.Try(marker);
			if (pose.valid_position == 0)
				return;

			var position = pose.position.v3();
			var animator = Animator;
			animator.SetIKHintPositionWeight(goal, 1);
			animator.SetIKHintPosition(goal, position);
		}
	}
}
