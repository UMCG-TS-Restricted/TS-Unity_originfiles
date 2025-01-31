using UnityEngine;


namespace ADG
{
	public static class Ease
	{
		public delegate float Func(float t);


		public static float One(Func func, float t) => func(Mathf.Clamp01(t));

		public static float To(float from, float to, float t, Func func) =>
			from + (to - from) * func(Mathf.Clamp01(t));


		public static float Linear(float t) => t;

		// TODO: Create BerpInOut
		public static float BerpOut(float t) =>
			(Mathf.Sin(t * Mathf.PI * (0.2f + 2.5f * t * t * t))
				* Mathf.Pow(1f - t, 2.2f) + t) * (1f + 1.2f * (1f - t));

		public static float Bounce(float t) =>
			Mathf.Abs(Mathf.Sin(6.28f * (t + 1f) * (t + 1f)) * (1f - t));

		public static float Sietse0(float t) => Mathf.Pow(t * 4, 0.5f / t) / 2.0f;

		public static float Hermite(float t) => t * t * (3.0f - 2.0f * t);


		public static float Quad(float t)
		{
			t *= 2.0f;
			if (t < 1)
				return QuadIn(t) / 2.0f;

			t -= 1.0f;
			return -(t * (t - 2.0f) - 1.0f) / 2.0f;
		}

		public static float QuadIn(float t) =>
			t * t;

		public static float QuadOut(float t) =>
			-t * (t - 2.0f);


		public static float Cubic(float t)
		{
			t *= 2.0f;
			if (t < 1.0f)
				return CubicIn(t) / 2.0f;

			t = t - 2.0f;
			return (t * t * t + 2.0f) / 2.0f;
		}

		public static float CubicIn(float t) =>
			t * t * t;

		public static float CubicOut(float t)
		{
			t -= 1.0f;
			return t * t * t + 1.0f;
		}


		public static float Quart(float t)
		{
			t *= 2.0f;

			if (t < 1.0f)
				return QuartIn(t) / 2.0f;

			t -= 2;
			return -(t * t * t * t - 2) / 2.0f;
		}

		public static float QuartIn(float t) =>
			t * t * t * t;

		public static float QuartOut(float t)
		{
			t -= 1.0f;
			return -(t * t * t * t - 1.0f);
		}


		public static float Quint(float t)
		{
			t *= 2.0f;
			if (t < 1.0f)
				return QuintIn(t) / 2.0f;

			t -= 2;
			return (t * t * t * t * t + 2) / 2.0f;
		}

		public static float QuintIn(float t) =>
			t * t * t * t * t;

		public static float QuintOut(float t)
		{
			t -= 1.0f;
			return t * t * t * t * t + 1.0f;
		}


		public static float Sine(float t) =>
			-(Mathf.Cos(Mathf.PI * t) - 1.0f) / 2.0f;

		public static float SineIn(float t) =>
			-Mathf.Cos(t * Mathf.PI / 2.0f) + 1;

		public static float SineOut(float t) =>
			Mathf.Sin(t * Mathf.PI / 2.0f);


		public static float Exp(float t)
		{
			t *= 2.0f;
			if (t < 1.0f)
				return ExpIn(t) / 2.0f;

			t -= 1.0f;
			return (-Mathf.Pow(2.0f, -10.0f * t) + 2.0f) / 2.0f;
		}

		public static float ExpIn(float t) =>
			Mathf.Pow(2.0f, 10.0f * (t - 1.0f));

		public static float ExpOut(float t) =>
			-Mathf.Pow(2.0f, -10.0f * t) + 1;


		public static float Circ(float t)
		{
			t *= 2.0f;
			if (t < 1.0f)
				return CircIn(t) / 2.0f;

			t -= 2.0f;
			return (Mathf.Sqrt(1 - t * t) + 1) / 2.0f;
		}

		public static float CircIn(float t) =>
			-(Mathf.Sqrt(1.0f - t * t) - 1.0f);

		public static float CircOut(float t)
		{
			t -= 1.0f;
			return Mathf.Sqrt(1.0f - t * t);
		}
	}
}
