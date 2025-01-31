using System;
using System.Collections.Generic;
using insitu;
using UnityEngine;


[CreateAssetMenu(fileName = "App", menuName = "football/Shared App Data")]
public class App : insitu.App
{
	[NonSerialized] public Telemetry Telemetry;
	[NonSerialized] public List<Hitter> Hitters;
	[NonSerialized] public List<PlaybackActor> Actors;
	[NonSerialized] public int Score;

	public Playback PlaybackAsset;
}
