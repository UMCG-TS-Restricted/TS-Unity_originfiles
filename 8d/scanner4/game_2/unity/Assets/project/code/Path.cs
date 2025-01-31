using System.IO;
using ADG;
using insitu;
using UnityEngine;


public struct Path
{
	public const float DefaultDistance = 10;
	public const float DefaultWalkThrough = 0.1f;
	public const float DefaultSpread = 0;
	public const float DefaultMinOffset = 0.5f;
	public const float DefaultMaxOffset = 0.8f;
	public const float DefaultSideChance = 0.5f;
	public const float DefaultFirstSideChance = 0.5f;
	public const int DefaultSwerveCount = 2;
	public const float DefaultDistribution = 1.0f;
	public const float DefaultAmplitude = 1.0f;
	public const float DefaultConvergence = 1.0f;

	public const ushort TypeId = 0xE1A4;
	public const float Overshoot = 3000.0f;


	public Footballer target;
	public int id;
	public int style;
	public float speed;
	public float wait;
	public float created_at;
	public array<ControlPoint> points;


	public Vector3 Evaluate(float moved, float overshoot, out int valid)
	{
		valid = 0;
		var start = Vector3.zero;
		for (var c = 0; c < 2; c++)
		{
			for (var i = 1; i < points.length; i++)
			{
				var p0 = points[i - 1];
				var p1 = points[i];
				const int iti = 32;
				const float itf = iti;
				for (var j = 0; j < iti; j++)
				{
					var alpha0 = (j + 0) / itf;
					var alpha1 = (j + 1) / itf;
					var position0 = ControlPoint.evaluate(p0, p1, alpha0);
					var position1 = ControlPoint.evaluate(p0, p1, alpha1);
					var delta = position1 - position0;
					var magnitude = delta.magnitude;
					if (magnitude > moved)
					{
						var alpha = moved / magnitude;
						alpha = Mathf.LerpUnclamped(alpha0, alpha1, alpha);
						return start + ControlPoint.evaluate(p0, p1, alpha);
					}

					moved -= magnitude;
				}
			}

			start += points.last.position - points[0].position;
			if (moved >= overshoot)
			{
				moved = overshoot;
				valid = -1;
			}
			else
			{
				valid = 1;
			}
		}

		valid = -1;
		return start + points.last.position;
	}

	public int Evaluate(float time, float deltaTime)
	{
		if (!target)
			return -1;

		var elapsed = time - (created_at + wait);
		if (elapsed < 0)
			elapsed = 0;

		var moved = elapsed * speed;
		var position = Evaluate(moved, Overshoot, out var move_next);
		var position_next = Evaluate(moved + 2, Overshoot, out _);
		target.CurrentPosition = position;
		target.TargetPosition = position_next;
		target.Apply(time, deltaTime);
		return move_next;
	}

	public void Apply(float time)
	{
		if (!target)
			return;

		if (time < created_at)
		{
			target.gameObject.SetActive(false);
			return;
		}

		var elapsed = time - (created_at + wait);
		if (elapsed < 0)
			elapsed = 0;

		var moved = elapsed * speed;
		var position = Evaluate(moved, Overshoot, out var move_next);
		if (move_next >= 0)
		{
			target.CurrentPosition = position;
			target.TargetPosition = points.last.position;
			target.Apply(time, Time.unscaledDeltaTime);
			target.gameObject.SetActive(true);
		}
		else target.gameObject.SetActive(false);

	}

	public static array<ControlPoint> Generate(Json.Object fp, uint seed, ControlPoint[] cache)
	{
		float distance = fp.NumberOf("distance", DefaultDistance);
		float walk_through = DefaultWalkThrough;
		float spread = fp.NumberOf("spread", DefaultSpread);
		float min_offset = fp.NumberOf("end_offset_min", DefaultMinOffset);
		float max_offset = fp.NumberOf("end_offset_max", DefaultMaxOffset);
		float side_chance = fp.NumberOf("end_side_chance", DefaultSideChance);
		float first_side_chance = fp.NumberOf("swerve_side_chance", DefaultFirstSideChance);
		int swerve_count = fp.NumberOf("swerve_count", DefaultSwerveCount);
		float distribution = fp.NumberOf("swerve_distribution", DefaultDistribution);
		float amplitude = fp.NumberOf("swerve_amplitude", DefaultAmplitude);
		float convergence = fp.NumberOf("swerve_factor", DefaultConvergence);
		return Generate(cache, seed, distance, walk_through, spread, min_offset, max_offset, side_chance, first_side_chance, swerve_count, distribution, amplitude, convergence);
	}

	public static array<ControlPoint> Generate(
		ControlPoint[] cache,
		uint seed,
		float distance,
		float walk_through,
		float spread,
		float min_offset,
		float max_offset,
		float side_chance,
		float first_side_chance,
		int swerve_count,
		float distribution,
		float amplitude,
		float convergence)
	{
		if (swerve_count < 0)
			swerve_count = 0;

		var angle = Hash.Noise(seed, 16654739U) * spread - spread / 2.0f;
		var rotation = Quaternion.AngleAxis(angle, Vector3.up);
		var start_position = rotation * new Vector3(0, 0, distance);
		var offset = Mathf.LerpUnclamped(min_offset, max_offset, Hash.Noise(seed, 3832773017U));
		if (Hash.Noise(seed, 16094459U) < side_chance)
			offset = -offset;
		var end_position = rotation * new Vector3(offset, 0, -walk_through);

		var capacity = swerve_count + 2;
		if (cache == null || cache.Length < capacity)
			cache = new ControlPoint[capacity];

		var result = new array<ControlPoint>
		{
			elements = cache,
			length = capacity,
		};

		var last = capacity - 1;
		for (var i = 0; i < capacity; i++)
		{
			var alpha = (float)i / last;
			alpha = Mathf.Pow(alpha, distribution);
			result[i] = new ControlPoint
			{
				position = Vector3.LerpUnclamped(start_position, end_position, alpha),
			};
		}

		var total_delta = end_position - start_position;
		var total_distance = total_delta.magnitude;
		var direction = total_delta;
		if (total_distance > 0)
			direction /= total_distance;

		for (var i = 0; i < capacity - 1; i++)
		{
			var alpha = (float)i / last;
			alpha = Mathf.Pow(alpha, distribution);
			var next = (float)(i + 1) / last;
			next = Mathf.Pow(next, distribution);
			var delta = next - alpha;
			var entry = result[i];
			entry.vector = direction * delta * total_distance * 0.5f;
			result[i] = entry;
		}

		var check = Hash.Noise(seed, 3531976781U) < first_side_chance ? 1 : 0;
		for (var i = 0; i < swerve_count; i++)
		{
			var value = amplitude * Mathf.Pow(convergence, i);
			if ((i & 1) == check)
				value = -value;
			var off = new Vector3(value, 0, 0);
			var entry = result[i + 1];
			entry.position += rotation * off;
			result[i + 1] = entry;
		}

		// Set last vector
		{
			var p0 = result[capacity - 2];
			var p1 = result[capacity - 1];
			p1.vector = p0.vector;
			result[capacity - 1] = p1;
		}

		return result;
	}

	public static Path Write(Telemetry telemetry, Path path)
	{
		if (path.id == 0)
			path.id = telemetry.EntityId();

		telemetry.Begin(TypeId, Telemetry.FlagBody | Telemetry.FlagEntity, 1, id: path.id);
		telemetry.Write(path.style);
		telemetry.Write(path.speed);
		telemetry.Write(path.wait);
		telemetry.Write(path.created_at);

		var points = path.points;
		telemetry.Write(points.length);
		for (var i = 0; i < points.length; i++)
		{
			var point = points[i];
			telemetry.Write(point.position);
			telemetry.Write(point.vector);
		}

		telemetry.End();
		return path;
	}

	public static bool Read(insitu.telemetry.Object obj, ref Path path)
	{
		if (obj.type != TypeId)
			return false;

		path.id = obj.entity;

		var reader = obj.Read(default);
		reader = reader.read(out path.style);
		reader = reader.read(out path.speed);
		reader = reader.read(out path.wait);
		reader = reader.read(out path.created_at);
		reader = reader.read(out int length);

		path.points = path.points.Reuse(length);
		for (var i = 0; i < length; i++)
		{
			ControlPoint point;
			reader = reader.read(out point.position);
			reader = reader.read(out point.vector);
			path.points[i] = point;
		}

		return true;
	}

	public readonly Json.Object json()
	{
		var array = new Json.Array();
		for (var i = 0; i < points.length; i++)
		{
			var point = points[i];
			array.Add(point.json());
		}

		return new Json.Object
		{
			{"header", TypeId },
			{"type", "path" },
			{"id", id },
			{"style", style },
			{"speed", speed },
			{"wait", wait },
			{"created_at", created_at },
			{"points", array },
		};
	}
}
