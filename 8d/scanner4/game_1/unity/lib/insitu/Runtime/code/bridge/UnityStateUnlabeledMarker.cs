using System;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		Marker reference to an unlabeled Vicon marker.
	///		This has to be used with caution as there is no control if the Id retains its value, or if it swaps with another instance.
	///		To make this useful, this has to be queried at runtime.
	/// </summary>
	public class UnityStateUnlabeledMarker : PoseBehaviour, IPoseSource
	{
		[NonSerialized] public int Index;
		[NonSerialized] public uint CachedId;
		[NonSerialized] public Unlabeled Marker;

		[Note("Marker reference to an unlabeled Vicon marker. This has to be used with caution as there is no control if the Id retains its value, or if it swaps with another instance. To make this useful, this has to be queried at runtime.")]
		public App App;
		public uint Id;

		public void ApplyCurrent()
		{
			gameObject.name = Index < 0
				? $"unlabeled {Id} (not found)"
				: $"unlabeled {Id}";

			transform.position = Marker.unity_position.v3();
		}

		public bool Scan(State state)
		{
			CachedId = Id;
			Index = state.IndexOfUnlabeled(Id);
			return Index >= 0;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(state);

		public bool Fetch(State state)
		{
			Scan(state);
			if (Index < 0)
				return false;

			var marker = state.unlabeled[Index];
			Marker = marker;
			return true;
		}


		public void Update()
		{
			if (Fetch())
				ApplyCurrent();
		}

		public override pose Pose()
		{
			if (Fetch())
				return Marker.unity_pose;

			return new pose
			{
				position = double3.from(transform.position),
				valid_position = 0,
				rotation = double4.from(transform.rotation),
				valid_rotation = 0,
			};
		}

		public pose PoseSource() => Fetch() ? Marker.vicon_pose : pose.identity;

		public void OnDrawGizmosSelected()
		{
			var pose = Pose();
			if (pose.valid_position == 0)
				return;

			var position = pose.position.v3();
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(position, 0.05f);
		}
	}
}