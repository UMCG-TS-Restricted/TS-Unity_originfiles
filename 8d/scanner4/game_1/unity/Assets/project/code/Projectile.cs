using System;
using System.Collections;
using ADG;
using insitu;
using UnityEngine;


public class Projectile : Hittable
{
	public struct Data
	{
		public const ushort Header = 0xE73C;

		public int id;
		public int state;
		public int flags;
		public float created_at;
		public float speed;
		public ControlPoint p0;
		public ControlPoint p1;
	}

	[NonSerialized] public Data State;
	[NonSerialized] public Material[] Materials;

	public MeshRenderer[] Indicators;

	public void OnDestroy()
	{
		if (Materials != null)
		{
			for (var i = 0; i < Materials.Length; i++)
				Destroy(Materials[i]);

			Materials = null;
		}
	}

	public void Color(Color color, Color emission)
	{
		if (Materials == null)
		{
			var indicators = Indicators;
			var materials = new Material[indicators.Length];
			for (var i = 0; i < indicators.Length; i++)
			{
				var indicator = indicators[i];
				var material = indicator.sharedMaterial;
				materials[i] = Instantiate(material);
				indicator.sharedMaterial = materials[i];
			}
			Materials = materials;
		}

		for (var i = 0; i < Materials.Length; i++)
		{
			var material = Materials[i];
			material.SetColor("_BaseColor", color);
			material.SetColor("_Color", color);
			material.SetColor("_EmissionColor", emission);
		}
	}

	public override void Hit(Hitter hitter)
	{
		var state = State;
		if (state.state != 0)
			return;

		var app = hitter.App;
		var hitters = app.Hitters;
		if (hitters == null)
			return;

		var index = hitters.IndexOf(hitter);
		if (index < 0)
			return;

		var flag = 1 << index;
		if ((state.flags & flag) != 0)
		{
			app.Score += app.Streak * 2;
			app.Score += 10;
			app.Streak++;
		}
		else
		{
			app.Score += 0;
			app.Lives--;
			app.Streak = 0;
		}

		state.state = 1;
		State = state;
		Destroy(gameObject);
	}

	public IEnumerator Despawn()
	{
		var time = Time.time;
		var start_position = transform.localPosition;
		var start_scale = transform.localScale;
		for (;;)
		{
			var delta = Time.time - time;
			var alpha = delta / 0.3f;
			if (alpha > 1)
				break;

			var scale = Ease.QuadOut(1 - alpha);
			transform.localScale = start_scale * scale;

			var position = -Ease.QuadIn(alpha);
			transform.localPosition = start_position + new Vector3(0, position, 0);
			yield return null;
		}

		Destroy(gameObject);
	}


	public static void Write(Telemetry telemetry, Projectile projectile)
	{
		if (projectile.State.id == 0)
			projectile.State.id = telemetry.EntityId();


		var state = projectile.State;
		telemetry.Begin(Data.Header, Telemetry.FlagEntity | Telemetry.FlagBody, 1, id: state.id);
		telemetry.Write(state.state);
		telemetry.Write(state.flags);
		telemetry.Write(state.created_at);
		telemetry.Write(state.speed);
		telemetry.Write(state.p0.position);
		telemetry.Write(state.p0.vector);
		telemetry.Write(state.p1.position);
		telemetry.Write(state.p1.vector);
		telemetry.End();
	}

	public bool MoveNext(float time)
	{
		var state = State;
		var lifetime = time - state.created_at;
		var alpha = lifetime * state.speed;
		if (state.state != 0 || alpha < 0 || alpha > 1)
			return false;

		var position = ControlPoint.evaluate(state.p0, state.p1, alpha);
		var position_next = ControlPoint.evaluate(state.p0, state.p1, alpha + 0.05f);
		transform.position = position;

		var direction = position_next - position;
		transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);
		return true;
	}

	public static bool Read(insitu.telemetry.Object obj, out Data data)
	{
		data = default;
		if (obj.type != Data.Header)
			return false;

		data.id = obj.entity;
		var reader = obj.Read(default);
		reader = reader.read(out data.state);
		reader = reader.read(out data.flags);
		reader = reader.read(out data.created_at);
		reader = reader.read(out data.speed);
		reader = reader.read(out data.p0.position);
		reader = reader.read(out data.p0.vector);
		reader = reader.read(out data.p1.position);
		reader = reader.read(out data.p1.vector);
		return true;
	}
}
