using System;
using System.Collections.Generic;
using ADG;
using insitu;
using UnityEngine;


[CreateAssetMenu]
public class App : insitu.App
{
	[NonSerialized] public Telemetry Telemetry;
	[NonSerialized] public Ease.Func EaseFunc;
	[NonSerialized] public List<Hitter> Hitters;
	[NonSerialized] public int Score;
	[NonSerialized] public int Streak;
	[NonSerialized] public int Lives;

	public Playback PlaybackAsset;
}
