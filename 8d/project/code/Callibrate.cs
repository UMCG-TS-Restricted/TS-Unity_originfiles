using System;
using ADG;
using insitu;
using TMPro;
using UnityEngine;


public class Callibrate : MonoBehaviour
{
	[NonSerialized] public float OverrideAlpha;
	[NonSerialized] public float Alpha;
	[NonSerialized] public int State;
	[NonSerialized] public float SnapshotTimer;
	[NonSerialized] public float BaseTimer;
	[NonSerialized] public Material Material;
	[NonSerialized] public Color MaterialColor;

	public bool SkipIfPossible;
	public bool UseSpaceForward;
	public PoseTransform Head;
	public PoseBehaviour Left;
	public PoseBehaviour Right;
	public Vector3 Offset = new Vector3(0.0f, 0.0f, 1.0f);
	public Canvas Canvas;
	public TMP_Text Text;
	public Transform Follow;
	public CanvasGroup Group;
	public Renderer Fadeout;
	public Camera Camera;
	public LayerMask CallibrateMask;
	public LayerMask DefaultMask;



	public void Start()
	{
		Canvas.enabled = false;
		State = 0;
		OverrideAlpha = 1;

		if (Fadeout)
		{
			var material = Fadeout.sharedMaterial;
			MaterialColor = material.color;
			Material = Instantiate(material);
			Fadeout.sharedMaterial = Material;
		}
	}

	public void OnEnable()
	{
		State = 0;
	}

	public void Initialize(Main main)
	{
		if (!SkipIfPossible)
			return;

		var input = Head.Input as UnityStateSegment;
		if (input == null)
		{
			Debug.LogWarning("Failed to detect if a callibration pass can be skipped: The input of Head is not of type UnityStateSegment");
			return;
		}

		if (!input.SavedReference)
		{
			Debug.LogWarning("Failed to detect if a callibration pass can be skipped: The input of Head is not marked as savable. Enable SavedReference to turn on this feature.");
			return;
		}

		if (input.Markers == null || input.Markers.Count == 0)
		{
			Debug.LogWarning("Failed to detect if a callibration pass can be skipped: Initialize was called before the head has been found.");
			return;
		}

		var app = main.App;
		var settings = app.Settings;
		if (input.Reference.length <= 0)
		{
			if (!input.LoadReference(settings, out var exists))
			{
				if (exists)
					Debug.LogWarning("Failed to detect if a callibration pass can be skipped: head reference was found, but has invalid data.");
				
				return;
			}
		}

		var rotation_offset = settings.ArrayOf("camera_rotation_offset");
		if (rotation_offset != null && rotation_offset.Count >= 4)
		{
			var rotation = double4.from(rotation_offset, 0, double4.identity);
			Head.transform.localRotation = rotation.q();
			main.CurrentState = Main.StateMenu;
			main.QueueScoreCheck = true;
		}
	}

	public void Update()
	{
		if (Alpha <= 0)
			State = 0;

		var enable_canvas = Alpha > 0.0001f;
		if (!enable_canvas)
			Canvas.enabled = false;

		var alpha = Ease.Hermite(Alpha);
		Group.alpha = alpha;
		Group.blocksRaycasts = alpha >= 0.9999f;

		if (Fadeout)
		{
			var fade = Mathf.Max(Alpha, OverrideAlpha);
			fade = Ease.Hermite(fade);
			if (fade > 0.95f)
			{
				Camera.cullingMask = CallibrateMask;
			}
			else
			{
				Camera.cullingMask = DefaultMask;
			}

			if (fade > 0.001f)
			{
				var color = MaterialColor;
				color.a = fade;
				Material.color = color;
				Fadeout.gameObject.SetActive(true);
			}
			else
			{
				Fadeout.gameObject.SetActive(false);
			}
		}
		else
		{
			Camera.cullingMask = DefaultMask;
		}

		if (Head)
		{
			var pose = Head.Pose();
			var position = pose.position.v3();
			var rotation = pose.rotation.q();
			position += rotation * Offset;

			var scale = new Vector3(alpha, alpha, alpha);
			Follow.localScale = scale;
			Follow.position = position;
		}
		else
		{
			Follow.localScale = Vector3.zero;
		}

		if (enable_canvas)
			Canvas.enabled = true;
	}

	public bool TrySnapshot(App app)
	{
		var right = Vector3.right;
		if (!UseSpaceForward && Left && Right)
		{
			var left_pose = Left.Pose();
			var right_pose = Right.Pose();
			if (left_pose.valid_position == 0 && right_pose.valid_position == 0)
				return false;

			var left_position = left_pose.position;
			var right_position = right_pose.position;
			var r_delta = right_position - left_position;
			var r = new Vector3((float)r_delta.x, 0, (float)r_delta.z);
			right = r;
		}

		var pose = Head.Pose();
		if (pose.valid_rotation == 0)
			return false;

		var up = Vector3.up;
		var fwd = Vector3.Cross(right, up);
		var forward = Vector3.Normalize(new Vector3(fwd.x, 0, fwd.z));
		var actual_look = Quaternion.LookRotation(forward, up);
		var rotation = pose.rotation.q();
		var delta_look = actual_look * Quaternion.Inverse(rotation);
		Head.transform.localRotation = delta_look;

		var settings = app.Settings;
		if (settings != null)
		{
			settings["camera_rotation_offset"] = new Json.Array
			{
				delta_look.x,
				delta_look.y,
				delta_look.z,
				delta_look.w,
			};
			app.Save();
		}

		return true;
	}

	public void UpdateActive(Main main, float deltaTime)
	{
		var time = Time.unscaledTime;
		var app = main.App;
		if (!App.FetchState(app))
		{
			UpdateInactive(deltaTime, false);
			BaseTimer = time;
			return;
		}

		Alpha += deltaTime / 0.6f;
		if (Alpha >= 1.0f)
			Alpha = 1.0f;
		else BaseTimer = time;

		OverrideAlpha += deltaTime / 0.6f;
		if (OverrideAlpha > 1)
			OverrideAlpha = 1;

		if (State == 0)
		{
			Text.text = "Hold a T-Pose\nLook forward";
			Text.color = Color.white;
			Text.transform.localScale = Vector3.one;
			if (time - BaseTimer > 5.0f)
			{
				State = 1;
				SnapshotTimer = time;
			}
		}
		else
		{
			var snap_delta = time - SnapshotTimer;
			var snap_float = 6 - snap_delta;
			var snap_int = (int)snap_float;
			if (snap_int > 3)
			{
				Text.text = "Hold still";
				Text.color = Color.white;
				Text.transform.localScale = Vector3.one;
			}
			else if (snap_int > 0)
			{
				var snap_alpha = snap_float - snap_int;
				snap_alpha = Mathf.InverseLerp(0.1f, 0.6f, snap_alpha);
				snap_alpha = Ease.CubicOut(snap_alpha);
				Text.text = snap_int.ToString();
				Text.color = new Color(1, 1, 1, snap_alpha);
				Text.transform.localScale = new Vector3(snap_alpha, snap_alpha, snap_alpha);
			}
			else
			{
				if (Head)
				{
					Head.transform.localRotation = Quaternion.identity;
					TrySnapshot(app);
				}

				if (State == 3)
				{
					Text.text = "Callibrated!";
				}
				else
				{
					Text.text = "Hold";
					State = 2;
				}

				Text.color = Color.white;
				Text.transform.localScale = Vector3.one;
				main.CurrentState = Main.StateMenu;
				main.QueueScoreCheck = true;
			}
		}
	}

	public void UpdateInactive(float deltaTime, bool valid)
	{
		Alpha -= deltaTime / 0.6f;
		if (Alpha < 0.0f)
			Alpha = 0.0f;

		if (valid)
		{
			OverrideAlpha -= deltaTime / 0.6f;
			if (OverrideAlpha < 0)
				OverrideAlpha = 0;
		}
		else
		{
			OverrideAlpha += deltaTime / 0.6f;
			if (OverrideAlpha > 1)
				OverrideAlpha = 1;
		}
	}
}
