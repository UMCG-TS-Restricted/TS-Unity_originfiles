using System;
using UnityEngine;
using static insitu.Vicon;

namespace insitu
{
	public class Player : MonoBehaviour
	{
		[NonSerialized] public int NexusId;
		[NonSerialized] public int SubjectIndex;
		[NonSerialized] public int LeftFootIndex;
		[NonSerialized] public int RightFootIndex;
		[NonSerialized] public int LeftHandIndex;
		[NonSerialized] public int RightHandIndex;
		[NonSerialized] public int LeftKneeIndex;
		[NonSerialized] public int RightKneeIndex;
		[NonSerialized] public int LeftElbowIndex;
		[NonSerialized] public int RightElbowIndex;
		[NonSerialized] public int LeftHipIndex;
		[NonSerialized] public int RightHipIndex;

		public App App;
		public Animator Animator;

		public string SubjectName;
		public string LeftFootName;
		public string RightFootName;
		public string LeftHandName;
		public string RightHandName;
		public string LeftKneeName;
		public string RightKneeName;
		public string LeftElbowName;
		public string RightElbowName;
		public string LeftHipName;
		public string RightHipName;


		public void Reset()
		{
			App = Unity.FindResource<App>();
			Animator = GetComponent<Animator>();
			if (!Animator) Animator = GetComponentInChildren<Animator>();
			SubjectName    = "";
			LeftFootName   = "LANK";
			RightFootName  = "RANK";
			LeftHandName   = "LFIN";
			RightHandName  = "RFIN";
			LeftKneeName   = "LKNE";
			RightKneeName  = "RKNE";
			LeftElbowName  = "LELB";
			RightElbowName = "RELB";
			LeftHipName    = "LPSI";
			RightHipName   = "RPSI";
		}

		public void Initialize(State state)
		{
			var subject_index = state.SubjectWith(SubjectName);
			if (subject_index < 0)
			{
				SubjectIndex = -1;
				return;
			}

			var subject = state.subjects[subject_index];
			var slice   = subject.markers;
			SubjectIndex    = subject_index;
			LeftFootIndex   = state.MarkerOf(slice, LeftFootName);
			RightFootIndex  = state.MarkerOf(slice, RightFootName);
			LeftHandIndex   = state.MarkerOf(slice, LeftHandName);
			RightHandIndex  = state.MarkerOf(slice, RightHandName);
			LeftKneeIndex   = state.MarkerOf(slice, LeftKneeName);
			RightKneeIndex  = state.MarkerOf(slice, RightKneeName);
			LeftElbowIndex  = state.MarkerOf(slice, LeftElbowName);
			RightElbowIndex = state.MarkerOf(slice, RightElbowName);
			LeftHipIndex    = state.MarkerOf(slice, LeftHipName);
			RightHipIndex   = state.MarkerOf(slice, RightHipName);
		}


		
		public void OnAnimatorIK(int layerIndex)
		{
			var app = App;
			var worker = app.Worker;
			var state = worker.State;
			if (NexusId != worker.StateVersion)
			{
				Initialize(state);
				NexusId = worker.StateVersion;
			}

			if (NexusId == 0 || SubjectIndex < 0)
				return;

			var markers = state.markers;
			{
				var hipli = markers[LeftHipIndex];
				var hipl = hipli.position.v3();
				var hipri = markers[RightHipIndex];
				var hipr = hipri.position.v3();
				if (hipli.valid + hipri.valid == 2)
				{
					var right = (hipr - hipl).normalized;
					var forward = Vector3.Cross(right, Vector3.up);
					transform.forward = forward;
					transform.position = (hipl + hipr) / 2 + new Vector3(0, 1.6f, 0);
				}
			}

			UpdateIK(markers, LeftHandIndex, AvatarIKGoal.LeftHand);
			UpdateIK(markers, RightHandIndex, AvatarIKGoal.RightHand);
			UpdateIK(markers, LeftFootIndex, AvatarIKGoal.LeftFoot);
			UpdateIK(markers, RightFootIndex, AvatarIKGoal.RightFoot);
			UpdateHintIK(markers, LeftKneeIndex, AvatarIKHint.LeftKnee);
			UpdateHintIK(markers, RightKneeIndex, AvatarIKHint.RightKnee);
			UpdateHintIK(markers, LeftElbowIndex, AvatarIKHint.LeftElbow);
			UpdateHintIK(markers, RightElbowIndex, AvatarIKHint.RightElbow);
		}

		public void UpdateIK(array<Marker> markers, int index, AvatarIKGoal goal)
		{
			var marker = markers.At(index);
			if (marker.valid == 0)
				return;

			var position = marker.position.v3();
			var animator = Animator;
			animator.SetIKPositionWeight(goal, 1);
			animator.SetIKPosition(goal, position);
		}

		public void UpdateHintIK(array<Marker> markers, int index, AvatarIKHint goal)
		{
			var marker = markers.At(index);
			if (marker.valid == 0)
				return;

			var position = marker.position.v3();
			var animator = Animator;
			animator.SetIKHintPositionWeight(goal, 1);
			animator.SetIKHintPosition(goal, position);
		}
	}
}
