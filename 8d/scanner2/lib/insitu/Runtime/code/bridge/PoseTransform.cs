namespace insitu
{
	/// <summary>
	///		Transform Input by:
	///		pose.position += pose.rotation * transform.position.
	///		pose.rotation *= transform.rotation.
	/// </summary>
	public class PoseTransform : PoseBehaviour
	{
		[Note("Transform Input by:\npose.position += pose.rotation * transform.position.\npose.rotation *= transform.rotation.")]
		public PoseBehaviour Input;

		public override pose Pose()
		{
			var input = Input;
			if (!input)
				return base.Pose();

			var t = transform;
			var pose = input.Pose();
			if (t && enabled)
			{
				var position = t.localPosition;
				var rotation = t.localRotation;
				if (pose.valid_rotation != 0)
					position = pose.rotation.q() * position;

				pose.position += position;
				pose.rotation *= rotation;
			}

			return pose;
		}
	}
}