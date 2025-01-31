using System;
using System.Globalization;
using ADG;
using insitu;
using UnityEngine;


public class Footballer : MonoBehaviour
{
	public const int MaterialIndexSkin = 0;
	public const int MaterialIndexClothesSecondary = 1;
	public const int MaterialIndexClothesPrimary = 2;
	public const int MaterialIndexHair = 3;
	public const int MaterialIndexBallSecondary = 0;
	public const int MaterialIndexBallPrimary = 1;
	public const int MaterialIndexBallTertiary = 1;
	public const int MaterialIndexEyeWhite = 0;
	public const int MaterialIndexEyeIris = 1;
	public const int MaterialIndexEyePupil = 2;
	public const int MaterialIndexBrow = 0;

	[NonSerialized] public int AnimatorHashAngle;
	[NonSerialized] public int AnimatorHashSpeed;
	[NonSerialized] public int AnimatorHashIdle;
	[NonSerialized] public Quaternion BallRotation;
	[NonSerialized] public Vector3 BallVelocity;
	[NonSerialized] public Vector3 LookAtVelocity;
	[NonSerialized] public float AngleCurrent;
	[NonSerialized] public float AngleVelocity;
	[NonSerialized] public float LookAtAlphaVelocity;
	[NonSerialized] public Vector3 CachedBallVelocity;
	[NonSerialized] public Vector3 CachedBallAngularVelocity;
	[NonSerialized] public Vector3 TargetPosition;
	[NonSerialized] public Vector3 CurrentPosition;

	public FootballerAnimator Animator;
	public Transform Ball;
	public Button BallHittable;
	public Rigidbody BallRigidbody;
	public Transform BallAnchor;
	public Transform FootAnchor;
	public Renderer BodyMesh;
	public Renderer BallMesh;
	public Renderer LeftEyeMesh;
	public Renderer RightEyeMesh;
	public Renderer BrowMesh;

	public void Awake()
	{
		BallRotation = UnityEngine.Random.rotation;
		AnimatorHashAngle = UnityEngine.Animator.StringToHash("angle");
		AnimatorHashSpeed = UnityEngine.Animator.StringToHash("speed");
		AnimatorHashIdle = UnityEngine.Animator.StringToHash("idle");
		CloneMaterials(BodyMesh);
		CloneMaterials(BallMesh);
		CloneMaterials(LeftEyeMesh);
		CloneMaterials(RightEyeMesh);
		CloneMaterials(BrowMesh);
	}

	public void OnDestroy()
	{
		DestroyMaterials(BodyMesh);
		DestroyMaterials(BallMesh);
		DestroyMaterials(LeftEyeMesh);
		DestroyMaterials(RightEyeMesh);
		DestroyMaterials(BrowMesh);
	}

	[ContextMenu("Detach Ball")]
	public Rigidbody DetachBall()
	{
		Debug.Log(CachedBallVelocity + " 000 " + CachedBallAngularVelocity);

		var rb = BallRigidbody;
		rb.isKinematic = false;
		rb.velocity = CachedBallVelocity * Time.fixedDeltaTime;
		//rb.angularVelocity = CachedBallAngularVelocity;
		return rb;
	}

	public void Apply(float time, float deltaTime)
	{
		var footballer = Animator;
		var animator = footballer.Animator;
		var state = animator.GetCurrentAnimatorStateInfo(0);
		var transition = animator.GetAnimatorTransitionInfo(0);
		var ball_rb = BallRigidbody;
		
		// Movement
		Vector3 target_position;
		{
			target_position = TargetPosition;
			var actor = footballer.transform;
			var current = actor.position;
			var delta = target_position - current;
			if (delta.sqrMagnitude > 0)
			{
				var angle_target = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
				var angle_current = Mathf.SmoothDampAngle(AngleCurrent, angle_target, ref AngleVelocity, 0.2f, 1000, deltaTime);
				if (deltaTime > 0)
				{
					var angle_delta = Mathf.DeltaAngle(AngleCurrent, angle_current);
					var angle_velocity = angle_delta / deltaTime;
					var angle_animator = angle_velocity * 0.16f;
					angle_animator = angle_animator * angle_animator * angle_animator;
					animator.SetFloat(AnimatorHashAngle, angle_animator);
				}

				AngleCurrent = angle_current;
			}
			
			var position = CurrentPosition;
			var movement = position - current;
			var magnitude = movement.magnitude;
			var velocity = 0.0f;
			if (deltaTime > 0)
			{
				velocity = magnitude / deltaTime;
				animator.SetFloat(AnimatorHashSpeed, velocity);
			}

			var animation_speed = velocity * 0.4f;
			if (animation_speed < 0.2f)
				animation_speed = 0.2f;

			if (state.shortNameHash == AnimatorHashIdle)
			{
				var alpha = transition.normalizedTime;
				animation_speed = Mathf.Lerp(1, animation_speed, alpha);
			}
			else
			{
				var alpha = transition.normalizedTime;
				animation_speed = Mathf.Lerp(animation_speed, 1, alpha);
			}

			var rotation = Quaternion.AngleAxis(AngleCurrent, Vector3.up);
			actor.SetPositionAndRotation(position, rotation);
			animator.speed = animation_speed;
		}

		// look
		{
			var ball_current = Ball.position;
			var ball_target = target_position;
			var ball_delta = ball_target - ball_current;
			var ball_clamped = Vector3.ClampMagnitude(ball_delta, 2.0f);
			var ball_distance = ball_clamped.sqrMagnitude;

			var target_alpha = Hash.Noise(time * 2, 2926564601U);
			target_alpha = target_alpha * target_alpha;
			target_alpha *= 0.75f;
			if (!ball_rb.isKinematic)
				target_alpha *= 0.2f;

			if (Hash.Noise(time, 2579081801U) < 0.65f && ball_distance < 2)
				target_alpha = 0;

			footballer.LookAtCurrent = Vector3.SmoothDamp(footballer.LookAtCurrent, ball_clamped, ref LookAtVelocity, 0.1f, 1000, deltaTime);
			footballer.LookAtAlphaCurrent = Mathf.SmoothDamp(footballer.LookAtAlphaCurrent, target_alpha, ref LookAtAlphaVelocity, 0.25f, 1000, deltaTime);
		}

		// Ball
		if (ball_rb.isKinematic)
		{
			var ball_anchor = BallAnchor;
			var ball_anchor_position = ball_anchor.position;
			var ball_anchor_alpha = transition.normalizedTime;
			if (state.shortNameHash == AnimatorHashIdle)
				ball_anchor_alpha = 1 - ball_anchor_alpha;

			var foot = FootAnchor;
			var foot_position = foot.position;
			foot_position.y = ball_anchor_position.y;
			var smooth_time = Mathf.LerpUnclamped(0.06f, 0.01f, ball_anchor_alpha);
			ball_anchor_position = Vector3.Lerp(ball_anchor_position, foot_position, ball_anchor_alpha);

			var ball = Ball;
			var ball_collider = ball.GetComponent<SphereCollider>();
			var ball_radius = ball_collider.radius;
			var ball_position = ball.position;

			var position = Vector3.SmoothDamp(ball_position, ball_anchor_position, ref BallVelocity, smooth_time, 1000, deltaTime);
			var delta = position - ball_position;
			var distance_sqr = (position - ball_anchor_position).sqrMagnitude;

			var linear_speed = deltaTime * 2;
			var linear_speed_sqr = linear_speed * linear_speed;
			var linear_delta = ball_anchor_position - ball_position;
			var linear_distance_sqr = linear_delta.sqrMagnitude;
			if (linear_speed_sqr > distance_sqr && linear_distance_sqr > linear_speed_sqr)
			{
				var linear_distance = Mathf.Sqrt(linear_distance_sqr);
				var linear_mul = linear_speed / linear_distance;
				var linear_position = ball_position + linear_delta * linear_mul;
				position = linear_position;
			}

			var delta_sqrm = delta.sqrMagnitude;
			if (delta_sqrm > 0)
			{
				var delta_magnitude = Mathf.Sqrt(delta_sqrm);
				var angle = delta_magnitude / (2 * ball_radius * Mathf.PI);
				var direction = delta / delta_magnitude;
				var normal = Vector3.up;
				var axis = Vector3.Cross(normal, direction);
				var rotation = Quaternion.AngleAxis(360 * angle, axis);
				ball.rotation = rotation * ball.rotation;

				if (deltaTime > 0)
				{
					CachedBallAngularVelocity = rotation.eulerAngles * Mathf.Deg2Rad / deltaTime;
					CachedBallVelocity = direction / deltaTime;
				}
			}

			ball.position = position;
		}
	}

	public void Initialize(Json.Object settings)
	{
		Initialize(settings, "color_skin", BodyMesh, MaterialIndexSkin);
		Initialize(settings, "color_clothes_secondary", BodyMesh, MaterialIndexClothesSecondary);
		Initialize(settings, "color_clothes_primary", BodyMesh, MaterialIndexClothesPrimary);
		Initialize(settings, "color_hair", BodyMesh, MaterialIndexHair);
		Initialize(settings, "color_ball_secondary", BallMesh, MaterialIndexBallSecondary);
		Initialize(settings, "color_ball_primary", BallMesh, MaterialIndexBallPrimary);
		Initialize(settings, "color_ball_tertiary", BallMesh, MaterialIndexBallTertiary);
		Initialize(settings, "color_left_eye_white", LeftEyeMesh, MaterialIndexEyeWhite);
		Initialize(settings, "color_left_eye_iris", LeftEyeMesh, MaterialIndexEyeIris);
		Initialize(settings, "color_left_eye_pupil", LeftEyeMesh, MaterialIndexEyePupil);
		Initialize(settings, "color_right_eye_white", RightEyeMesh, MaterialIndexEyeWhite);
		Initialize(settings, "color_right_eye_iris", RightEyeMesh, MaterialIndexEyeIris);
		Initialize(settings, "color_right_eye_pupil", RightEyeMesh, MaterialIndexEyePupil);
		Initialize(settings, "color_brow", BrowMesh, MaterialIndexBrow);
	}

	public static void DestroyMaterials(Renderer renderer)
	{
		var materials = renderer.materials;
		for (var i = 0; i < materials.Length; i++)
			Destroy(materials[i]);
	}

	public static void CloneMaterials(Renderer renderer)
	{
		var materials_original = renderer.sharedMaterials;
		var materials = new Material[materials_original.Length];
		for (var i = 0; i < materials.Length; i++)
			materials[i] = materials_original[i];
		renderer.materials = materials;
	}

	public static void Initialize(Json.Object settings, string key, Renderer renderer, int index)
	{
		if (settings == null)
			return;

		var value = settings.StringOf(key);
		if (value == null)
			return;

		if (uint.TryParse(value.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
		{
			var color32 = new Color32(
				(byte)((result >> 24) & 0xFF),
				(byte)((result >> 16) & 0xFF),
				(byte)((result >>  8) & 0xFF),
				(byte)((result >>  0) & 0xFF));
			var materials = renderer.sharedMaterials;
			GizmoData.Apply(materials[index], color32);
		}
		else
		{
			Debug.LogWarning($"Failed to parse {value} of {key} in {settings}. It expects a string value that represents a hex integer that fits inside a unsigned 32-bit integer.");
		}
	}
}
