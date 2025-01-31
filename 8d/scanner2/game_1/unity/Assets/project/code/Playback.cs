using System;
using System.Collections.Generic;
using ADG;
using insitu;
using UnityEngine;


[ExecuteInEditMode]
public class Playback : MonoBehaviour
{
	[NonSerialized] public insitu.telemetry.File File;
	[NonSerialized] public float CurrentTime;
	[NonSerialized] public float VelocityTime;
	[NonSerialized] public List<PlaybackHitter> Hitters;
	[NonSerialized] public List<Projectile> Projectiles;
	[NonSerialized] public Vicon.State State;

	[HideInInspector] public string FilePath;
	[HideInInspector] public float TargetTime;

	public PlaybackHitter HitterAsset;
	public Projectile ProjectileAsset;
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
		if (Hitters != null)
		{
			for (var i = 0; i < Hitters.Count; i++)
			{
				var hitter = Hitters[i];
				hitter.Active = false;
			}
		}

		if (Projectiles != null)
		{
			for (var i = 0; i < Projectiles.Count; i++)
			{
				var projectile = Projectiles[i];
				projectile.State.state = 1;
			}
		}
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

		if (Projectiles != null)
		{
			for (var i = 0; i < Projectiles.Count; i++)
			{
				var projectile = Projectiles[i];
				var active = projectile.MoveNext(CurrentTime);
				projectile.gameObject.SetActive(active);
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

		if (ParseProjectile(obj))
			return 0;

		var count = Vicon.State.Read(objects, index, File.cached_strings, ref State);
		if (count > 0)
			return count - 1;

		return 0;
	}

	public bool ParseProjectile(insitu.telemetry.Object obj)
	{
		if (obj.type != Projectile.Data.Header)
			return false;

		if (!Projectile.Read(obj, out var data))
			return false;

		Projectile projectile = null;
		var projectiles = Projectiles;
		if (projectiles == null)
		{
			Projectiles = projectiles = new List<Projectile>();
		}
		else
		{
			for (var i = 0; i < projectiles.Count; i++)
			{
				var other = projectiles[i];
				if (other.State.id == obj.entity)
				{
					projectile = other;
					break;
				}
			}
		}

		if (projectile == null)
		{
			projectile = Instantiate(ProjectileAsset, transform);
			projectile.State = data;
			projectiles.Add(projectile);
		}

		projectile.State = data;
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
			else if (Projectile.Read(obj, out var projectile))
			{
				current.Add(new Json.Object
				{
					{"header", obj.type },
					{"version", obj.version},
					{"type", "projectile" },
					{"id", projectile.id },
					{"state", projectile.state == 0 ? "alive" : "dead" },
					{"flags", projectile.flags },
					{"created_at", projectile.created_at },
					{"speed", projectile.speed },
					{"p0_position", double3.from(projectile.p0.position).json() },
					{"p0_vector", double3.from(projectile.p0.vector).json() },
					{"p1_position", double3.from(projectile.p1.position).json() },
					{"p1_vector", double3.from(projectile.p1.vector).json() },
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
