using System;
using UnityEngine;


public class Button : Hittable
{
	[NonSerialized] public Hitter Hitter;
	[NonSerialized] public float LastHit;

	public void OnEnable()
	{
		Hitter = null;
		LastHit = -1000;
	}

	public void OnDisable()
	{
		Hitter = null;
		LastHit = -1000;
	}

	public override void Hit(Hitter hitter)
	{
		LastHit = Time.unscaledTime;
		Hitter = hitter;
	}

	[ContextMenu("Click")]
	public void FakeClick()
	{
		Hitter = FindObjectOfType<Hitter>();
		LastHit = Time.unscaledTime;
		Debug.Log("Fake click");
	}
}
