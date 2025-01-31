using System;
using UnityEngine;

public class FootballerAnimator : MonoBehaviour
{
	[NonSerialized] public Vector3 LookAtCurrent;
	[NonSerialized] public float LookAtAlphaCurrent;

	public Animator Animator;

	public void OnAnimatorIK(int layerIndex)
	{
		var animator = Animator;
		animator.SetLookAtPosition(LookAtCurrent);
		animator.SetLookAtWeight(LookAtAlphaCurrent);
	}
}
