using System;
using System.Collections.Generic;
using System.Text;
using ADG;
using insitu;
using TMPro;
using UnityEngine;


public class Menu : MonoBehaviour
{
	public static readonly string[] Sizes = new string[]
	{
		"36", "28", "24", "18"
	};
	
	[NonSerialized] public float Alpha;

	public TMP_Text Text;
	public CanvasGroup LastScoreGroup;
	public TMP_Text LastScore;
	public Button Callibrate;
	public Button Game;
	public Canvas Canvas;
	public CanvasGroup Group;


	public void Update()
	{
		var enable_canvas = Alpha > 0.0001f;
		if (!enable_canvas)
			Canvas.enabled = false;

		var alpha = Ease.Hermite(Alpha);
		var scale = new Vector3(alpha, alpha, alpha);
		Group.alpha = alpha;
		Group.blocksRaycasts = alpha >= 0.9999f;
		Callibrate.transform.localScale = scale;
		Game.transform.localScale = scale;

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
		if (Alpha > 1.0f)
			Alpha = 1.0f;

		if (app.Score >= 0)
		{
			LastScore.text = app.Score.ToString();
			LastScoreGroup.alpha = 1;
		}
		else
		{
			LastScoreGroup.alpha = 0;
		}

		var time = Time.unscaledTime;
		var delta = time - Callibrate.LastHit;
		if (delta < 5.0f)
		{
			Callibrate.OnDisable();
			main.CurrentState = Main.StateCallibrate;
		}

		delta = time - Game.LastHit;
		if (delta < 5.0f)
		{
			Game.OnDisable();
			main.StartGame();
		}

		if (main.QueueScoreCheck)
		{
			main.QueueScoreCheck = false;
			var list = new List<ScoreEntry>();
			var save = main.Game.SaveData ??= global::Game.Load();
			var entries = save.EnsuredArrayOf("entries");
			for (var i = 0; i < entries.Count; i++)
			{
				var entry = entries.ObjectAt(i);
				var name = entry.StringOf("name");
				var score = entry.NumberOf("score");
				if (name == null || score == null)
					continue;

				list.Add(new ScoreEntry
				{
					name = name,
					score = score,
				});
			}

			if (list.Count < 3)
			{
				list.Add(new ScoreEntry
				{
					name = "William Clifford",
					score = 33,
				});
			}

			list.Sort((x, y) => y.score.CompareTo(x.score));

			var builder = new StringBuilder();
			for (var i = 0; i < list.Count && i < 10; i++)
			{
				var entry = list[i];
				var size = Sizes[i >= Sizes.Length ? Sizes.Length - 1 : i];
				builder.Append("<size=");
				builder.Append(size);
				builder.Append(">");
				builder.Append(entry.score);
				builder.Append(" ");
				builder.Append(entry.name);
				builder.Append("</size>\n");
			}

			Text.text = builder.ToString();
		}
	}

	public void UpdateInactive(float deltaTime)
	{
		Alpha -= deltaTime / 0.6f;
		if (Alpha < 0.0f)
			Alpha = 0.0f;
	}

	public struct ScoreEntry
	{
		public string name;
		public int score;
	}
}
