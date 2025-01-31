using System.IO;
using ADG;
using insitu;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(App))]
public class AppEditor : insitu.AppEditor
{
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
		var app = (App)target;
		var settings = app.FetchSettings();
		var end_position = Vector3.zero;
		end_position.x += settings["center_position_x"];
		end_position.y += settings["center_position_y"];
		end_position.z += settings["center_position_z"];

		var spawn_spread = (float)settings["spawn_spread"];
		var spread_arc = Quaternion.AngleAxis(-spawn_spread / 2, Vector3.up) * Vector3.forward;
		Handles.DrawSolidArc(end_position, Vector3.up, spread_arc, spawn_spread, 1.0f);
		//Gizmos.DrawSphere(end_position, settings["center_radius"]);

		var start_position = new Vector3(0, 0, settings["spawn_distance"]);
		start_position.y += settings["spawn_height"];
		//Gizmos.DrawSphere(end_position, settings["spawn_radius"]);

		var curve = (float)settings["max_curve"];
		var curve_pitch_from = Quaternion.AngleAxis(curve, Vector3.right);
		var curve_pitch_to = Quaternion.AngleAxis(curve, Vector3.left);
		var rotation_from = curve_pitch_from;
		var rotation_to = curve_pitch_to;
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

		const int iti = 16;
		const float itf = iti;
		for (var i = 0; i < iti; i++)
		{
			var alpha0 = (i + 0) / itf;
			var alpha1 = (i + 1) / itf;
			var position0 = ControlPoint.evaluate(p0, p1, alpha0);
			var position1 = ControlPoint.evaluate(p0, p1, alpha1);
			Handles.DrawLine(position0, position1);
		}
	}


	public override void OnInspectorGUISettings()
	{
		var app = (App)target;
		var settings = app.FetchSettings();

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.LabelField("Vicon Settings");
		settings["host"] = EditorGUILayout.TextField("Host", settings.StringOf("host", "127.0.0.1"));
		settings["port"] = EditorGUILayout.IntField("Port", settings.NumberOf("port", 801));
		settings["streaming_mode"] = EditorGUILayout.Popup("Vicon Mode", settings.NumberOf("streaming_mode", 1), StreamingModes);
		settings["vicon_mode"] = EditorGUILayout.Popup("Vicon Mode", settings.NumberOf("vicon_mode", 1), ViconModes);
		settings["scale"] = EditorGUILayout.FloatField("Scale", settings.NumberOf("scale", 1));

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Vicon Settings");
		settings["player_name"] = EditorGUILayout.TextField("Player Display Name", settings.StringOf("player_name", ""));
		settings["seed"] = EditorGUILayout.IntField("Game Seed", settings.NumberOf("seed"));
		settings["rotation_offset"] = EditorGUILayout.FloatField("Room Orientation", settings.NumberOf("rotation_offset"));
		settings["any_weight"] = EditorGUILayout.FloatField("Block Any Chance", settings.NumberOf("any_weight"));
		settings["single_weight"] = EditorGUILayout.FloatField("Block Specific Chance", settings.NumberOf("single_weight")); 
		settings["start_lives"] = EditorGUILayout.IntField("Lives on Start", settings.NumberOf("start_lives", 5));
		settings["start_speed"] = EditorGUILayout.FloatField("Speed on Start", settings.NumberOf("start_speed", 0.1f));
		settings["acceleration"] = EditorGUILayout.FloatField("Acceleration on Start", settings.NumberOf("acceleration", 0.01f));
		settings["spawn_distance"] = EditorGUILayout.FloatField("Spawn Distance", settings.NumberOf("spawn_distance", 10));
		settings["spawn_height"] = EditorGUILayout.FloatField("Spawn Height", settings.NumberOf("spawn_height", 1));
		settings["spawn_radius"] = EditorGUILayout.FloatField("Spawn Radius", settings.NumberOf("spawn_radius", 1));
		settings["spawn_spread"] = EditorGUILayout.Slider("Spawn Spread", settings.NumberOf("spawn_spread", 53), 0 , 360);
		settings["min_wait"] = EditorGUILayout.FloatField("Spawn Interval Min", settings.NumberOf("min_wait", 0.4f));
		settings["max_wait"] = EditorGUILayout.FloatField("Spawn Interval Max", settings.NumberOf("max_wait", 0.4f));
		settings["min_curve"] = EditorGUILayout.FloatField("Curvature Min", settings.NumberOf("min_curve", 0.0f));
		settings["max_curve"] = EditorGUILayout.FloatField("Curvature Max", settings.NumberOf("max_curve", 0.0f));
		var center_position_x = (float)settings.NumberOf("center_position_x", 0.0f);
		var center_position_y = (float)settings.NumberOf("center_position_y", 0.5f);
		var center_position_z = (float)settings.NumberOf("center_position_z", 0.0f);
		var center_position = new Vector3(center_position_x, center_position_y, center_position_z);
		center_position = EditorGUILayout.Vector3Field("Center Position", center_position);
		settings["center_radius"] = EditorGUILayout.FloatField("Center Radius", settings.NumberOf("center_radius", 0.5f));
		if (EditorGUI.EndChangeCheck())
		{
			settings["center_position_x"] = center_position.x;
			settings["center_position_y"] = center_position.y;
			settings["center_position_z"] = center_position.z;

			var content = settings.Stringify(Json.Pretty);
			SettingsString = content;
			File.WriteAllText(App.SettingsPath, content);
			SceneView.RepaintAll();
		}
	}
}
