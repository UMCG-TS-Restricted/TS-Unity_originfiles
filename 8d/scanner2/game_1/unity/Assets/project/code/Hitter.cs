using System.Collections.Generic;
using UnityEngine;
using insitu;
using System;


[RequireComponent(typeof(Rigidbody))]
public class Hitter : MonoBehaviour
{
	public const ushort TypeId = 0xE134;

	[NonSerialized] public Collider[] Colliders;
	[NonSerialized] public float Alpha;
	[NonSerialized] public int EntityId;
	[NonSerialized] public Vector3[] Axis;
	[NonSerialized] public float[] Speed;


	public App App;
	public PoseBehaviour Pose;
	[ColorUsage(false, true)]
	public Color Color;
	[Range(0, 1)]
	public float Weight;
	public float Radius;
	public Transform[] Rings;

	public void Awake()
	{
		Colliders = new Collider[16];
		Alpha = 0;
		Radius = 0.1f;
		Axis = new Vector3[Rings.Length];
		for (var i = 0; i < Axis.Length; i++)
			Axis[i] = UnityEngine.Random.onUnitSphere;
		Speed = new float[Rings.Length];
		for (var i = 0; i < Speed.Length; i++)
			Speed[i] = UnityEngine.Random.Range(8.0f, 12.0f);

		Apply();
	}

	public void OnValidate()
	{
		if (Radius < 0)
			Radius = 0;
	}

	public void Hide(float deltaTime)
	{
		Alpha -= deltaTime / 0.4f;
		if (Alpha < 0)
			Alpha = 0;
	}

	public void Apply()
	{
		Color.a = Alpha;
	}

	public void Update()
	{
		for (var i = 0; i < Rings.Length; i++)
		{
			var r = Rings[i];
			var t = Time.time * Speed[i];
			r.localRotation = Quaternion.AngleAxis(t, Axis[i]);
		}

		var deltaTime = Time.deltaTime;
		if (!Pose)
		{
			Hide(deltaTime);
			return;
		}

		var pose = Pose.Pose();
		if (pose.valid_position == 0)
		{
			Hide(deltaTime);
			return;
		}

		Alpha += deltaTime / 0.4f;
		if (Alpha > 1)
			Alpha = 1;

		var rb = GetComponent<Rigidbody>();
		rb.MovePosition(pose.position.v3());
		Apply();
	}

	public void FixedUpdate()
	{
		if (!Pose)
			return;

		var pose = Pose.Pose();
		if (pose.valid_position == 0)
			return;

		var position = pose.position.v3();
		var colliders = Colliders;
		var count = Physics.OverlapSphereNonAlloc(position, Radius, colliders);
		for (var i = 0; i < count; i++)
		{
			var collider = colliders[i];
			var hittable = collider.GetComponent<Hittable>();
			if (hittable)
				hittable.Hit(this);
		}
	}

	public void OnDrawGizmos()
	{
		if (!transform)
			return;

		Gizmos.color = Color;
		Gizmos.DrawWireSphere(transform.position, Radius);
	}


	public void Reset()
	{
		App = insitu.Unity.FindResource<App>();
		Color = Color.white;
		Weight = 1;
	}

	public void OnEnable()
	{
		var hitters = App.Hitters;
		if (hitters == null)
			App.Hitters = hitters = new List<Hitter>();

		hitters.Add(this);
	}

	public void OnDisable()
	{
		var hitters = App.Hitters;
		if (hitters != null)
			hitters.Remove(this);
	}

	public static void Write(Telemetry telemetry, Hitter hitter)
	{
		if (hitter.EntityId == 0)
			hitter.EntityId = telemetry.EntityId();

		telemetry.Begin(TypeId, Telemetry.FlagBody | Telemetry.FlagEntity, 1, id: hitter.EntityId);
		telemetry.Write(hitter.Color);
		telemetry.Write(hitter.Radius);
		telemetry.Write(hitter.Alpha);
		telemetry.Write(hitter.transform.position);
		telemetry.End();
	}

	public static bool Read(insitu.telemetry.Object obj, out Color color, out float radius, out float alpha, out Vector3 position)
	{
		color = default;
		radius = default;
		alpha = default;
		position = default;
		if (obj.type != TypeId)
			return false;

		var reader = obj.Read(default);
		reader = reader.read(out color);
		reader = reader.read(out radius);
		reader = reader.read(out alpha);
		reader = reader.read(out position);
		return true;
	}

}
