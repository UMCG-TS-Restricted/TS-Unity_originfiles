using System.Globalization;
using System.IO;
using ADG;
using insitu;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(App))]
public class AppEditor : insitu.AppEditor
{
	public int PreviewIndex = -1;
	public ControlPoint[] Cache;

	public static readonly string[] GameModes =
	{
		"Foot",
		//"Block",
	};

	public override void OnEnable()
	{
		base.OnEnable();
		SceneView.duringSceneGui += OnScene;
	}

	public void OnDisable()
	{
		SceneView.duringSceneGui -= OnScene;
	}

	public void OnScene(SceneView view)
	{
		if (PreviewIndex < 0)
			return;

		var app = (App)target;
		var settings = app.FetchSettings();
		var footballer_paths = settings.EnsuredArrayOf("footballer_paths");
		if (PreviewIndex >= footballer_paths.Count)
			return;

		var fp = footballer_paths.ObjectAt(PreviewIndex);
		if (fp == null)
			return;

		uint seed = settings.NumberOf("seed", 0);
		var path = Path.Generate(fp, seed, Cache);
		Cache = path.elements;

		for (var i = 0; i < path.length - 1; i++)
		{
			var p0 = path[i + 0];
			var p1 = path[i + 1];
			const int iti = 8;
			const float itf = iti;
			for (var j = 0; j < iti; j++)
			{
				var alpha0 = (j + 0) / itf;
				var alpha1 = (j + 1) / itf;
				var position0 = ControlPoint.evaluate(p0, p1, alpha0);
				var position1 = ControlPoint.evaluate(p0, p1, alpha1);
				Handles.DrawLine(position0, position1);
			}
		}
	}

	public static Color32 ColorOf(uint c) => new Color32(
		(byte)((c >> 24) & 0xFF),
		(byte)((c >> 16) & 0xFF),
		(byte)((c >>  8) & 0xFF),
		(byte)((c >>  0) & 0xFF));

	public static uint UintOf(Color32 c) => ((uint)c.r << 24) | ((uint)c.g << 16) | ((uint)c.b << 8) | ((uint)c.a << 0);

	public static void ColorField(Json.Object settings, string key, string name, Color32 color)
	{
		if (settings == null)
			return;

		uint cache_val = UintOf(color);
		var value = settings.StringOf(key);
		if (value != null)
		{
			if (uint.TryParse(value.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
			{
				cache_val = result;
				color = ColorOf(result);
			}
		}

		var color_float = EditorGUILayout.ColorField(name, color);
		var color_new = (Color32)color_float;
		var color_code = UintOf(color_new);
		if (cache_val != color_code)
			settings[key] = color_code.ToString("X8");
	}

	public override void OnInspectorGUISettings()
	{
		var app = (App)target;
		var settings = app.FetchSettings();
		var force_change = false;

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.LabelField("Vicon Settings");
		settings["host"] = EditorGUILayout.TextField("Host", settings.StringOf("host", "127.0.0.1"));
		settings["port"] = EditorGUILayout.IntField("Port", settings.NumberOf("port", 801));
		settings["streaming_mode"] = EditorGUILayout.Popup("Vicon Mode", settings.NumberOf("streaming_mode", 1), StreamingModes);
		settings["vicon_mode"] = EditorGUILayout.Popup("Vicon Mode", settings.NumberOf("vicon_mode", 1), ViconModes);
		settings["scale"] = EditorGUILayout.FloatField("Scale", settings.NumberOf("scale", 1));
		settings["rotation_offset"] = EditorGUILayout.FloatField("Room Orientation", settings.NumberOf("rotation_offset"));
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Game Settings");
		settings["player_name"] = EditorGUILayout.TextField("Player Display Name", settings.StringOf("player_name", ""));
		settings["seed"] = EditorGUILayout.IntField("Footballer Seed", settings.NumberOf("seed"));
		settings["game_mode"] = EditorGUILayout.Popup("Game Mode", settings.NumberOf("game_mode", 1), GameModes);
		settings["spawn_count"] = EditorGUILayout.IntField("Footballer Spawn Count", settings.NumberOf("spawn_count", 3));
		settings["speed"] = EditorGUILayout.FloatField("Footballer Speed", settings.NumberOf("speed", 3));
		settings["wait"] = EditorGUILayout.FloatField("Wait Before Run", settings.NumberOf("wait", 0.8f));

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Footballer Style");
		EditorGUILayout.HelpBox("You can customize footballer colors by adding/removing/changing styles. There can be referenced by index later.", MessageType.Info);
		{
			var delete_index = -1;
			var footballer_styles = settings.EnsuredArrayOf("footballer_styles");
			for (var i = 0; i < footballer_styles.Count; i++)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField($"Footballer Style {i}");
				var fs = footballer_styles.ObjectAt(i);
				if (fs == null)
					footballer_styles[i] = fs = new Json.Object();

				ColorField(fs, "color_skin", "Skin", ColorOf(0x4B2D15FFu));
				ColorField(fs, "color_hair", "Hair", ColorOf(0x1D1816FFu));
				ColorField(fs, "color_brow", "Brow", ColorOf(0x1D1816FFu));
				ColorField(fs, "color_left_eye_white", "Eye Left White", ColorOf(0xFFEFEFFFu));
				ColorField(fs, "color_left_eye_iris", "Eye Left Iris", ColorOf(0x914B1AFFu));
				ColorField(fs, "color_left_eye_pupil", "Eye Left Pupil", ColorOf(0x1A1418FFu));
				ColorField(fs, "color_right_eye_white", "Eye Right White", ColorOf(0xFFEFEFFFu));
				ColorField(fs, "color_right_eye_iris", "Eye Right Iris", ColorOf(0x914B1AFFu));
				ColorField(fs, "color_right_eye_pupil", "Eye Right Pupil", ColorOf(0x1A1418FFu));
				ColorField(fs, "color_clothes_primary", "Clothes Primary", ColorOf(0x000000FFu));
				ColorField(fs, "color_clothes_secondary", "Clothes Secondary", ColorOf(0xFFFFFFFFu));
				ColorField(fs, "color_ball_primary", "Ball Primary", ColorOf(0x3B68BBFFu));
				ColorField(fs, "color_ball_secondary", "Ball Secondary", ColorOf(0x303035FFu));
				ColorField(fs, "color_ball_tertiary", "Ball Tertiary", ColorOf(0xFF865DFFu));

				if (GUILayout.Button($"Delete Footballer Style {i}"))
					delete_index = i;
			}

			if (delete_index >= 0)
			{
				force_change = true;
				footballer_styles.RemoveAt(delete_index);
			}

			EditorGUILayout.Space();
			if (GUILayout.Button("Add Footballer Style"))
			{
				footballer_styles.Add(new Json.Object());
				force_change = true;
			}
		}


		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Footballer Path");
		EditorGUILayout.HelpBox("You can customize footballer run paths. " +
			"Note: When style is negative, it will be randomly selected from the defined list.", MessageType.Info);
		{
			var delete_index = -1;
			var footballer_paths = settings.EnsuredArrayOf("footballer_paths");
			for (var i = 0; i < footballer_paths.Count; i++)
			{
				EditorGUILayout.Space();
				if (PreviewIndex == i)
					EditorGUILayout.LabelField($"Footballer Path {i} (selected)");
				else EditorGUILayout.LabelField($"Footballer Path {i}");
				var fp = footballer_paths.ObjectAt(i);
				if (fp == null)
					footballer_paths[i] = fp = new Json.Object();

				fp["style"] = EditorGUILayout.IntField("Style Index", fp.NumberOf("style", -1));
				fp["distance"] = EditorGUILayout.FloatField("Spawn distance", fp.NumberOf("distance", Path.DefaultDistance));
				fp["spread"] = EditorGUILayout.Slider("Spawn Spread", fp.NumberOf("spread", Path.DefaultSpread), 0, 360);
				fp["end_offset_min"] = EditorGUILayout.FloatField("Left/Right target min distance", fp.NumberOf("end_offset_min", Path.DefaultMinOffset));
				fp["end_offset_max"] = EditorGUILayout.FloatField("Left/Right target max distance", fp.NumberOf("end_offset_max", Path.DefaultMaxOffset));
				fp["end_side_chance"] = EditorGUILayout.Slider("Chance of getting left side target", fp.NumberOf("end_side_chance", Path.DefaultSideChance), 0, 1);
				fp["swerve_side_chance"] = EditorGUILayout.Slider("First swerve left side chance", fp.NumberOf("swerve_side_chance", Path.DefaultFirstSideChance), 0, 1);
				fp["swerve_count"] = EditorGUILayout.IntField("Swerve Count", fp.NumberOf("swerve_count", Path.DefaultSwerveCount));
				fp["swerve_distribution"] = EditorGUILayout.FloatField("Point Distribution", fp.NumberOf("swerve_distribution", Path.DefaultDistribution));
				fp["swerve_amplitude"] = EditorGUILayout.FloatField("Swerve Amplitude", fp.NumberOf("swerve_amplitude", Path.DefaultAmplitude));
				fp["swerve_factor"] = EditorGUILayout.FloatField("Swerve Factor", fp.NumberOf("swerve_factor", Path.DefaultConvergence));

				if (GUILayout.Button($"Preview Footballer Style {i}"))
				{
					PreviewIndex = i;
					SceneView.RepaintAll();
				}

				if (GUILayout.Button($"Delete Footballer Style {i}"))
					delete_index = i;
			}

			if (delete_index >= 0)
			{
				force_change = true;
				footballer_paths.RemoveAt(delete_index);
			}

			EditorGUILayout.Space();
			if (GUILayout.Button("Add Footballer Style"))
			{
				footballer_paths.Add(new Json.Object());
				force_change = true;
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Block Game Mode Settings");
		EditorGUILayout.HelpBox("The Block Game Mode is a mode in which you need to put a marker in front of the footballer's path.", MessageType.Info);
		settings["block_radius"] = EditorGUILayout.FloatField("Block Correct Radius", settings.NumberOf("block_radius", 0.2f));
		settings["block_area"] = EditorGUILayout.FloatField("Block Active Radius", settings.NumberOf("block_area", 1.5f));

		if (EditorGUI.EndChangeCheck() || force_change)
		{
			var content = settings.Stringify(Json.Pretty);
			SettingsString = content;
			File.WriteAllText(App.SettingsPath, content);
			SceneView.RepaintAll();
		}
	}
}
