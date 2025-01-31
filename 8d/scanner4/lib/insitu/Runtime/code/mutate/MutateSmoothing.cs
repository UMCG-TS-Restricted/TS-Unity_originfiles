using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Fetches the pose of Input every frame and smooths the pose over time.
	/// </summary>
	public class MutateSmoothing : PoseBehaviour
	{
		[NonSerialized] public pose Current;
		[NonSerialized] public double4 Rotation;
		[NonSerialized] public double4 RotationVelocity;
		[NonSerialized] public double3 Position;
		[NonSerialized] public double3 PositionVelocity;

		public PoseBehaviour Input;
		public float Smoothing = 0.04f;
		[Tooltip("When true Pose will return the value directly provider by Input")]
		public bool Bypass;

		public void Awake()
		{
			Current = pose.identity;
		}

		public void Update()
		{
			var pose = Input.Pose();
			if (pose.valid_position != 0)
			{
				Position = pose.position;
				if (Current.valid_position == 0)
				{
					Current.position = pose.position;
					Current.valid_position = 1;
				}
			}
			if (pose.valid_rotation != 0)
			{
				Rotation = pose.rotation;
				if (Current.valid_rotation == 0)
				{
					Current.rotation = pose.rotation;
					Current.valid_rotation = 1;
				}
			}

			if (Smoothing < 0.001f)
				Smoothing = 0.001f;

			var deltaTime = Time.unscaledDeltaTime;
			if (Current.valid_position != 0)
				Current.position = double3.Smooth(Current.position, Position, ref PositionVelocity, deltaTime, Smoothing);

			if (Current.valid_rotation != 0)
			{
				Current.rotation = double4.Smooth(Current.rotation, Rotation, ref RotationVelocity, deltaTime, Smoothing);
			}
			
			Debug.Log(pose.json());
			Debug.Log(Current.json());
		}

		public override pose Pose()
		{
			if (Bypass)
				return Input.Pose();

			var result = Current;
			result.rotation = double4.normalizeq(result.rotation);
			return result;
		}
	}
}