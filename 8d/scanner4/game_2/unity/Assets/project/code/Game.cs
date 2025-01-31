using System;
using System.IO;
using ADG;
using insitu;
using TMPro;
using UnityEngine;


public class Game : MonoBehaviour
{
	[NonSerialized] public Json.Object SaveData;
	[NonSerialized] public float SpawnTime;
	[NonSerialized] public int SpawnIndex;
	[NonSerialized] public uint Seed;
	[NonSerialized] public float Alpha;
	[NonSerialized] public Path Current;
	[NonSerialized] public float PrevTime;
	[NonSerialized] public int Mode;
	[NonSerialized] public float EndMessageTime;

	public Footballer Asset;

	[Header("UI")]
	public Canvas Canvas;
	public CanvasGroup Group;
	public TMP_Text Info;

	[Header("Block Mode")]
	public PoseBehaviour BlockPose;


	public void Awake()
	{
		SaveData = Load();
	}

	public void Initialize(Main main)
	{
		var app = main.App;
		var settings = app.FetchSettings();
		uint seed = settings["seed"];
		if (seed == 0)
			seed = (uint)UnityEngine.Random.Range(13, int.MaxValue - 2048);

		SpawnTime = (float)app.Time() + 1.9f;
		SpawnIndex = 0;
		Seed = Hash.Simple(seed, 11690143U);
		Current = default;
		PrevTime = (float)app.Time();
		Clear();
		EndMessageTime = -1;
		Mode = settings["game_mode"];
	}

	public void Clear()
	{
		var current = Current;
		var target = current.target;
		if (target)
			Destroy(target.gameObject);
		// TODO: destroy animation
		Current = default;
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

		var time = (float)app.Time();
		var dt = time - PrevTime;
		PrevTime = time;

		var current = Current;
		var state = current.Evaluate(time, dt);
		if (state == 0)
		{
			if (Mode == 0)
			{
				var footballer = current.target;
				var ball_rb = footballer.BallRigidbody;
				if (ball_rb.isKinematic)
				{
					var ball = footballer.BallHittable;
					if (time < current.created_at + current.wait)
						ball.Hitter = null;

					if (ball.Hitter)
					{
						footballer.DetachBall();
						app.Score++;
						Info.text = "Ball successfully captured!";
						EndMessageTime = Time.time + 4.0f;
						Debug.Log("Ball successfully captured");
					}
				}
			}
			else
			{
				var footballer = current.target;
				var ball_rb = footballer.BallRigidbody;
				if (ball_rb.isKinematic)
				{
					var settings = app.FetchSettings();
					var pos = footballer.CurrentPosition;
					var tar = current.points.last.position;
					var del = tar - pos;
					var mag = del.magnitude;
					if (mag < settings.NumberOf("block_area", 1.5f))
					{
						footballer.DetachBall();
						app.Score++;
						Info.text = "Ball successfully captured!";
						EndMessageTime = Time.time + 4.0f;
						Debug.Log("Ball successfully captured");
						Clear();
					}
				}
			}
		}
		else if (state > 0)
		{
			Info.text = "Failed to capture the ball!";
			EndMessageTime = Time.time + 1.0f;
		}
		else if (current.target)
		{
			SpawnTime = time + 2.0f;
			Clear();
		}
		else if (time > SpawnTime)
		{
			array<ControlPoint> path;
			Json.Object style = null;
			var style_index = -1;
			var settings = app.FetchSettings();
			var spawn_index = SpawnIndex;
			var spawn_count = settings.NumberOf("spawn_count", 3);
			if (spawn_index >= spawn_count)
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
				var file_path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
				File.WriteAllText(file_path, text);
			}
			else
			{
				var footballer_styles = settings.EnsuredArrayOf("footballer_styles");
				var footballer_paths = settings.EnsuredArrayOf("footballer_paths");
				if (footballer_paths.Count > 0)
				{
					var footballer_index = SpawnIndex % footballer_paths.Count;
					var footballer_path = footballer_paths.ObjectAt(footballer_index);
					path = Path.Generate(footballer_path, Seed + (uint)SpawnIndex, null);

					style_index = footballer_path.NumberOf("style", -1);
					if (style_index >= 0 && style_index < footballer_styles.Count)
						style = footballer_styles.ObjectAt(style_index);
				}
				else
				{
					path = Path.Generate(null,
						Seed + (uint)SpawnIndex,
						Path.DefaultDistance,
						Path.DefaultWalkThrough,
						Path.DefaultSpread,
						Path.DefaultMinOffset,
						Path.DefaultMaxOffset,
						Path.DefaultSideChance,
						Path.DefaultFirstSideChance,
						Path.DefaultSwerveCount,
						Path.DefaultDistribution,
						Path.DefaultAmplitude,
						Path.DefaultConvergence);
				}

				if (style == null && footballer_styles.Count > 0)
				{
					var rand = Hash.Simple(Seed + (uint)SpawnIndex, 2743200253u);
					var index = rand % (uint)footballer_styles.Count;
					style_index = (int)index;
					style = footballer_styles.ObjectAt(style_index);
				}

				var footballer = Instantiate(Asset);
				footballer.Initialize(style);
				Clear();
				Current = new Path
				{
					created_at = time,
					points = path,
					speed = settings.NumberOf("speed", 3),
					style = style_index,
					wait = settings.NumberOf("wait", 0.8f),
					target = footballer,
				};
				SpawnIndex = spawn_index + 1;
			}
		}

		if (Time.time > EndMessageTime)
		{
			if (Mode == 0)
				Info.text = "Capture the ball!";
			else Info.text = "Stand near the center\nand block the player!";
		}
	}

	public void UpdateInactive(float deltaTime)
	{
		Alpha -= deltaTime / 0.6f;
		if (Alpha < 0.0f)
			Alpha = 0.0f;
	}

	public static Json.Object Load()
	{
		var path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
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
