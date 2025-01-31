using System;
using System.Collections.Generic;
using System.IO;
using ADG;
using insitu;
using TMPro;
using UnityEngine;


public class Game : MonoBehaviour
{
	[NonSerialized] public Json.Object SaveData;
	[NonSerialized] public List<Projectile> Projectiles;
	[NonSerialized] public float GameStart;
	[NonSerialized] public float NextSpawn;
	[NonSerialized] public uint SpawnIndex;
	[NonSerialized] public float Alpha;

	public Projectile Asset;

	[Header("UI")]
	public Canvas Canvas;
	public CanvasGroup Group;
	public TMP_Text Info;
	public Healthbar Healthbar;

	public void Awake()
	{
		Projectiles = new List<Projectile>();
		SaveData = Load();
	}

	public void Initialize(Main main)
	{
		var app = main.App;
		uint seed = app.Settings["seed"];
		if (seed == 0)
			seed = (uint)UnityEngine.Random.Range(13, int.MaxValue);

		app.Lives = app.Settings["start_lives"];
		app.Score = 0;
		app.Streak = 0;
		if (Healthbar)
			Healthbar.Initialize(app);
		NextSpawn = GameStart = Time.time + 4.9f;
		SpawnIndex = Hash.Simple(seed, 11690143U);
		Clear();
	}

	public void Clear()
	{
		var projectiles = Projectiles;
		for (var i = 0; i < projectiles.Count; i++)
		{
			var projectile = projectiles[i];
			if (projectile)
				Destroy(projectile.gameObject);
			// TODO: destroy animation
		}
		projectiles.Clear();
	}

	public void Update()
	{
		var enable_canvas = Alpha > 0.0001f;
		if (!enable_canvas)
			Canvas.enabled = false;

		var alpha = Ease.Hermite(Alpha);
		Group.alpha = alpha;
		Group.blocksRaycasts = alpha >= 0.9999f;

		if (enable_canvas)
			Canvas.enabled = true;
	}

	public void UpdateActive(Main main, float deltaTime)
	{
		var app = main.App;
		if (!App.FetchState(app))
		{
			UpdateInactive(deltaTime);
			return;
		}

		Alpha += deltaTime / 0.6f;
		if (Alpha >= 1.0f)
			Alpha = 1.0f;

		if (Healthbar)
			Healthbar.ActiveUpdate(app, deltaTime);

		var settings = app.Settings;
		var time = Time.time;
		var delta = GameStart - time;
		if (delta > 0)
		{
			var timer_number = (int)delta;
			var timer_scale = 0.006f + 0.004f * (delta - timer_number);
			var s = Ease.QuadIn(timer_scale);
			Info.enabled = true;
			Info.text = timer_number.ToString();
			Info.transform.localScale = new Vector3(s, s, s);
		}
		else
		{
			Info.enabled = false;
		}

		if (app.Lives < 0)
		{
			main.CurrentState = Main.StateMenu;
			main.QueueScoreCheck = true;
			Clear();

			var save = SaveData = Load();
			var entries = save.EnsuredArrayOf("entries");
			entries.Add(new Json.Object
			{
				{"score", app.Score },
				{"name", app.Settings["player_name"] },
			});
			var text = save.Stringify();
			var path = Path.Combine(Application.persistentDataPath, "save.json");
			File.WriteAllText(path, text);
		}

		var projectiles = Projectiles;

		if (time > NextSpawn)
		{
			var min_wait = Mathf.Max(settings["min_wait"], 0.05f);
			var max_wait = Mathf.Max(settings["max_wait"], min_wait);
			var wait = Mathf.Lerp(min_wait, max_wait, Hash.Noise(SpawnIndex, 10909601U));
			var angle = Hash.Noise(SpawnIndex, 16654739U) * settings["spawn_spread"] - settings["spawn_spread"] / 2.0f;
			var curve = Mathf.Lerp(settings["min_curve"], settings["max_curve"], Hash.Noise(SpawnIndex, 14184581U));
			var target_yew = 360 * Hash.Noise(SpawnIndex, 8702503U);
			var target_pitch = 360 * Hash.Noise(SpawnIndex, 13420387U);
			var target_offset = settings["center_radius"] * Hash.Noise(SpawnIndex, 11459627U);
			var spawn_yew = 360 * Hash.Noise(SpawnIndex, 13349857U);
			var spawn_pitch = 360 * Hash.Noise(SpawnIndex, 16094459U);
			var spawn_offset = settings["spawn_radius"] * Hash.Noise(SpawnIndex, 13295627U);

			var start_position = Vector3.zero;
			start_position += Quaternion.AngleAxis(angle, Vector3.up) * new Vector3(0, 0, settings["spawn_distance"]);
			start_position.y += settings["spawn_height"];
			start_position += VectorOf(spawn_yew, spawn_pitch, spawn_offset);

			var end_position = Vector3.zero;
			end_position.x += settings["center_position_x"];
			end_position.y += settings["center_position_y"];
			end_position.z += settings["center_position_z"];
			end_position += VectorOf(target_yew, target_pitch, target_offset);

			var speed = settings["start_speed"] + settings["acceleration"] * NextSpawn * 0.01f;


			if ((Hash.Simple(SpawnIndex, 2281740607U) & 1) == 0)
					curve = -curve;

			var curve_pitch_from = Quaternion.AngleAxis(curve, Vector3.right);
			var curve_pitch_to = Quaternion.AngleAxis(curve, Vector3.left);
			var curve_roll_angle = 360 * Hash.Noise(SpawnIndex, 3832773017U);
			var curve_roll = Quaternion.AngleAxis(curve_roll_angle, Vector3.forward);
			var rotation_from = curve_roll * curve_pitch_from;
			var rotation_to = curve_roll * curve_pitch_to;
			var position_delta = end_position - start_position;
			var magnitude = position_delta.magnitude;
			var direction = position_delta / magnitude;
			var factor = magnitude * 0.4f;
			var p0 = new ControlPoint
			{
				position = start_position,
				vector = rotation_from * direction * factor,
			};
			var p1 = new ControlPoint
			{
				position = end_position,
				vector = rotation_to * direction * factor,
			};


			var type = -1;
			var color = Color.white;
			var weight_any = (float)settings.NumberOf("any_weight", 0.2f);
			var weight_single = (float)settings.NumberOf("single_weight", 0.8f);
			var weight_total = weight_any + weight_single;
			var type_alpha = weight_total * Hash.Noise(SpawnIndex, 3531976781U);
			if (type_alpha <= weight_any)
			{
				type = -1;
				color = Color.white;
			}
			type_alpha -= weight_any;

			var hitters = app.Hitters;
			if (hitters != null && hitters.Count > 0 && type_alpha <= weight_single)
			{
				var hitter_total = 0.0f;
				for (var i = 0; i < hitters.Count; i++)
					hitter_total += hitters[i].Weight;

				var index = 0;
				var index_rnd = hitter_total * Hash.Noise(SpawnIndex, 2743200253U);
				for (var i = 0; i < hitters.Count; i++)
				{
					var element = hitters[i];
					index_rnd -= element.Weight;
					if (index_rnd <= 0)
					{
						index = i;
						break;
					}
				}

				var hitter = hitters[index];
				type = 1 << index;
				color = hitter.Color;
			}

			var state = new Projectile.Data
			{
				state = 0,
				flags = type,
				created_at = NextSpawn,
				speed = speed,
				p0 = p0,
				p1 = p1,
			};

			var instance = Instantiate(Asset);
			instance.State = state;
			instance.Color(color, color * 2);
			projectiles.Add(instance);

			NextSpawn += wait;
			SpawnIndex++;
		}


		for (var i = projectiles.Count - 1; i >= 0; i--)
		{
			var projectile = projectiles[i];
			if (!projectile || projectile.State.state == 1)
			{
				projectiles.RemoveAt(i);
				continue;
			}

			if (!projectile.MoveNext(time))
			{
				app.Lives--;
				app.Streak = 0;
				projectiles.RemoveAt(i);
				Destroy(projectile.gameObject);
			}
		}
	}

	public void UpdateInactive(float deltaTime)
	{
		Alpha -= deltaTime / 0.6f;
		if (Alpha < 0.0f)
			Alpha = 0.0f;
	}

	public static Vector3 VectorOf(float yew, float pitch, float distance) =>
		Quaternion.AngleAxis(yew, Vector3.up) *
		Quaternion.AngleAxis(pitch, Vector3.right) *
		new Vector3(0, 0, distance);

	public static Json.Object Load()
	{
		var path = Path.Combine(Application.persistentDataPath, "save.json");
		if (File.Exists(path))
		{
			var text = File.ReadAllText(path);
			var json = Json.ParseObject(text);
			if (json != null)
				return json;
		}

		return new Json.Object { };
	}
}
