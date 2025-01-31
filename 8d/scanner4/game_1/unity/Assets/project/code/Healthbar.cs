using System;
using UnityEngine;
using UnityEngine.UI;


public class Healthbar : MonoBehaviour
{
	[NonSerialized] public int MaxHealth;
	[NonSerialized] public int PreviousHealth;
	[NonSerialized] public float Current;
	[NonSerialized] public float Velocity;
	[NonSerialized] public Color Color;
	[NonSerialized] public Color ColorVelocity;

	public Gradient HealthColor;
	public Color HitColor;
	public RectTransform Bar;
	public Graphic[] Graphics;


	public void Initialize(App app)
	{
		MaxHealth = app.Lives;
		PreviousHealth = app.Lives;
		Current = 1;
		Velocity = 0;
		Color = HealthColor.Evaluate(1);
		ColorVelocity = Vector4.zero;
	}

	public void ActiveUpdate(App app, float deltaTime)
	{
		// Fill
		var target = (float)app.Lives / MaxHealth;
		var alpha = Current = Mathf.SmoothDamp(Current, target, ref Velocity, 0.2f, 10000, deltaTime);
		if (Mathf.Abs(target - alpha) <= 0.01f)
			PreviousHealth = app.Lives;

		// Color
		var target_color = HitColor;
		if (PreviousHealth <= app.Lives)
			target_color = HealthColor.Evaluate(alpha);
		var color = Color = SmoothDamp(Color, target_color, ref ColorVelocity, 0.2f, deltaTime);

		// Apply
		Bar.anchorMax = new Vector2(alpha, 1);
		for (var i = 0; i < Graphics.Length; i++)
			Graphics[i].color = color;
	}

	public static float Dot(Color a, Color b) => a.r * b.r + a.g * b.g + a.b * b.b + a.a * b.a;

	public static Color SmoothDamp(Color current, Color target, ref Color currentVelocity, float smoothTime, float deltaTime)
	{
		current.r = Mathf.SmoothDamp(current.r, target.r, ref currentVelocity.r, smoothTime, 1000, deltaTime);
		current.g = Mathf.SmoothDamp(current.g, target.g, ref currentVelocity.g, smoothTime, 1000, deltaTime);
		current.b = Mathf.SmoothDamp(current.b, target.b, ref currentVelocity.b, smoothTime, 1000, deltaTime);
		current.a = Mathf.SmoothDamp(current.a, target.a, ref currentVelocity.a, smoothTime, 1000, deltaTime);
		return current;
	}
}
