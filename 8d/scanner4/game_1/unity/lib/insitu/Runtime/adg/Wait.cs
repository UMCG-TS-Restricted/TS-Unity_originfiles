using System.Collections;
using UnityEngine;

namespace ADG
{
	public struct Wait : IEnumerator
	{
		public float Start;
		public float Duration;

		public float Target => Start + Duration;
		public float Delta => Time.time - Start;
		public float Remaining => Duration - (Time.time - Start);
		public float Alpha => Mathf.Clamp01((Time.time - Start) / Duration);
		public float AlphaInverse => Mathf.Clamp01(1.0f - (Time.time - Start) / Duration);
		public float UnclampedAlpha => (Time.time - Start) / Duration;
		public bool Finished => Time.time >= Start + Duration;

		public Wait(float duration)
		{
			Start = Time.time;
			Duration = duration;
		}

		/// <inheritdoc />
		public bool MoveNext() => Time.time < Start + Duration;

		/// <inheritdoc />
		public void Reset() => Start = Time.time;

		/// <inheritdoc />
		object IEnumerator.Current => null;
	}
}
