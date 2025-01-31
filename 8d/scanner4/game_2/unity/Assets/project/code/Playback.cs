using System;
using System.Collections.Generic;
using ADG;
using insitu;
using UnityEngine;


public class Playback : MonoBehaviour
{
	[NonSerialized] public insitu.telemetry.File File;
	[NonSerialized] public float CurrentTime;
	[NonSerialized] public float VelocityTime;
	[NonSerialized] public List<Path> Paths;
	[NonSerialized] public List<PlaybackHitter> Hitters;
	[NonSerialized] public List<PlaybackActor> Actors;
	[NonSerialized] public Vicon.State State;

	[HideInInspector] public string FilePath;
	[HideInInspector] public float TargetTime;

	public App App;
	public PlaybackHitter HitterAsset;
	public Footballer Footballer;
	public PlaybackActor Actor;
	public UnityState UnityState;

	public bool Range(out float min, out float max)
	{
		var file = File;
		var frames = file.frames;
		if (frames.length > 0)
		{
			min = frames[0].time;
			max = frames.last.time;
			return true;
		}

		min = 0;
		max = 1;
		return false;
	}


	public void Update()
	{
		if (File.data == null)
		{
			File = insitu.telemetry.File.Read(FilePath);
			if (File.data == null)
			{
				//Destroy(gameObject);
				return;
			}
		}

		Clean();

		CurrentTime = Mathf.SmoothDamp(CurrentTime, TargetTime, ref VelocityTime, 0.2f, 10000, Time.deltaTime);
		var frame_index = File.FrameAt(CurrentTime);
		var frames = File.frames;
		var frame = frames[frame_index];
		var block_index = frame.block_index;
		var block = File.blocks[block_index];
		var objects = File.objects;
		for (var i = 0; i < block.length; i++)
		{
			var block_frame_index = block.offset + i;
			if (block_frame_index == frame_index)
				break;

			var block_frame = frames[block_frame_index];
			var children = block_frame.children;
			for (var j = 0; j < children.length; j++)
			{
				var child_index = children.offset + j;
				j += Parse(objects, child_index, 1);
			}
		}

		if (frame_index < frames.length - 1 && CurrentTime > frame.time)
		{
			var next = frames[frame_index + 1];
			var alpha = Mathf.InverseLerp(frame.time, next.time, CurrentTime);
			var children = next.children;
			for (var j = 0; j < children.length; j++)
			{
				var child_index = children.offset + j;
				j += Parse(objects, child_index, alpha);
			}
		}

		Apply();
	}

	public void Clean()
	{
		// Destroy all things
	}

	public void Apply()
	{
		if (Hitters != null)
		{
			for (var i = 0; i < Hitters.Count; i++)
			{
				var hitter = Hitters[i];
				hitter.Apply(CurrentTime);
			}
		}

		if (Actors != null)
		{
			for (var i = 0; i < Actors.Count; i++)
			{
				var actor = Actors[i];
				actor.Apply(actor.Cache);
			}
		}

		if (Paths != null)
		{
			for (var i = 0; i < Paths.Count; i++)
			{
				var path = Paths[i];
				path.Apply(CurrentTime);
			}
		}

		if (State.version > 0)
			UnityState.Fetch(State);
	}

	public int Parse(array<insitu.telemetry.Object> objects, int index, float alpha)
	{
		var obj = objects[index];
		if (ParseHitter(obj, alpha))
			return 0;

		if (ParseActor(obj))
			return 0;

		if (ParsePath(obj))
			return 0;

		var count = Vicon.State.Read(objects, index, File.cached_strings, ref State);
		if (count > 0)
			return count - 1;

		return 0;
	}

	public bool ParsePath(insitu.telemetry.Object obj)
	{
		if (Paths == null)
			Paths = new List<Path>();

		if (obj.type != Path.TypeId)
			return false;

		Path path = default;
		var index = -1;
		var paths = Paths;
		for (var i = 0; i < paths.Count; i++)
		{
			var element = paths[i];
			if (element.id == obj.entity)
			{
				index = i;
				path = element;
				break;
			}
		}

		if (!Path.Read(obj, ref path))
			return false;

		if (index < 0)
		{
			Json.Object style = null;
			var settings = App.FetchSettings();
			var footballer_styles = settings.EnsuredArrayOf("footballer_styles");
			var style_index = path.style;
			if (style_index >= 0 && style_index < footballer_styles.Count)
				style = footballer_styles.ObjectAt(style_index);

			var footballer = Instantiate(Footballer, transform);
			footballer.Initialize(style);
			path.target = footballer;
			Paths.Add(path);
		}
		else
		{
			Paths[index] = path;
		}

		return true;
	}

	public bool ParseHitter(insitu.telemetry.Object obj, float alpha)
	{
		if (obj.type != Hitter.TypeId)
			return false;

		if (!Hitter.Read(obj, out var color, out var radius, out var hitter_alpha, out var position))
			return false;

		PlaybackHitter hitter = null;
		var hitters = Hitters;
		if (hitters == null)
		{
			Hitters = hitters = new List<PlaybackHitter>();
		}
		else
		{
			for (var i = 0; i < hitters.Count; i++)
			{
				var other = hitters[i];
				if (other.Id == obj.entity)
				{
					hitter = other;
					break;
				}
			}
		}

		if (hitter == null)
		{
			hitter = Instantiate(HitterAsset, transform);
			hitter.Id = obj.entity;
			hitters.Add(hitter);
		}

		hitter.Active = true;
		hitter.Color = Color.LerpUnclamped(hitter.Color, color, alpha);
		hitter.Radius = Mathf.LerpUnclamped(hitter.Radius, radius, alpha);
		hitter.Alpha = Mathf.LerpUnclamped(hitter.Alpha, hitter_alpha, alpha);
		hitter.Position = Vector3.LerpUnclamped(hitter.Position, position, alpha);
		return true;
	}

	public bool ParseActor(insitu.telemetry.Object obj)
	{
		if (obj.type != PlaybackActor.TypeId)
			return false;

		if (!PlaybackActor.Read(obj, out var data))
			return false;

		var actors = Actors;
		if (actors == null)
			Actors = actors = new List<PlaybackActor>();

		PlaybackActor actor = null;
		for (var i = 0; i < actors.Count; i++)
		{
			var element = actors[i];
			if (element.Id == obj.entity)
			{
				actor = element;
				break;
			}
		}

		if (!actor)
		{
			actor = Instantiate(Actor, transform);
			actor.Id = obj.entity;
			actors.Add(actor);
		}

		actor.Cache = data;
		return true;
	}


	public static Json.Object ToJson(insitu.telemetry.File file)
	{
		Vicon.State state = default;
		var frames = new Json.Array();
		var root = new Json.Object
		{
			{"metadata", file.metadata },
			{"frames", frames }
		};
		var current = frames;


		var objects = file.objects;
		for (var i = 0; i < objects.length; i++)
		{
			var obj = objects[i];
			if (insitu.telemetry.Frame.Read(obj, out var frame))
			{
				current = new Json.Array();
				frames.Add(new Json.Object
				{
					{"id", frame.id },
					{"time", frame.time },
					{"header", obj.type },
					{"type", "frame" },
					{"version", obj.version },
					{"entities", current },
				});
			}
			else if (Hitter.Read(obj, out var color, out var radius, out var hitter_alpha, out var position))
			{
				current.Add(new Json.Object
				{
					{"header", obj.type },
					{"version", obj.version },
					{"type", "hitter" },
					{"color", new Json.Object
					{
						{"r", color.r},
						{"g", color.g},
						{"b", color.b},
						{"a", color.a},
					}},
					{"radius", radius },
					{"alpha", hitter_alpha },
					{"position", double3.from(position).json() },
				});
			}
			else
			{
				var count = Vicon.State.Read(objects, i, file.cached_strings, ref state) - 1;
				if (count >= 0)
				{
					i += count;
					current.Add(state.ToJson(true));
				}
			}
		}
		return root;
	}
}
