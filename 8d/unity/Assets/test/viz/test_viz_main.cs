using System;
using insitu;
using UnityEngine;


public class test_viz_main : MonoBehaviour
{
	[Note("Test note")]
	[Header("Segment")]
	public Transform P0;
	public Transform P1;


	[Header("Corner")]
	public Transform C0;
	public Transform C1;
	public Transform C2;

	public Gizmo Gizmo;

	[NonSerialized] public ControlPoint[] Cache = new ControlPoint[8];

	private void OnDrawGizmos()
	{
		if (P0 && P1)
		{
			var c0 = new ControlPoint
			{
				position = P0.position,
				vector = P0.TransformVector(Vector3.forward),
			};

			var c1 = new ControlPoint
			{
				position = P1.position,
				vector = P1.TransformVector(Vector3.forward),
			};

			ControlPoint.DrawGizmo(c0, c1);
		}

		if (C0 && C1 && C2)
		{
			var c0 = new ControlPoint
			{
				position = C0.position,
				vector = C0.TransformVector(Vector3.forward),
			};

			var c1 = new ControlPoint
			{
				position = C1.position,
				vector = C1.TransformVector(Vector3.forward),
			};

			var c2 = new ControlPoint
			{
				position = C2.position,
				vector = C2.TransformVector(Vector3.forward),
			};

			Gizmos.color = Color.red;
			Gizmos.DrawLine(c0.position, c1.position);
			Gizmos.DrawLine(c2.position, c1.position);

			Gizmos.color = Color.white;
			Gizmo.Evaluate(c0.position, c1.position, c2.position, Vector3.up, 0.2f);

			var x = ControlPoint.arc(c0.position, c1.position, c2.position, Vector3.up, 0.2f, out var angle, Cache);
			for (var i = 0; i < x; i++)
			{
				//ControlPoint.DrawGizmo(Cache[i + 0], Cache[i + 1]);
			}

			UnityEditor.Handles.Label(c1.position, $"{angle * Mathf.Rad2Deg:0.00}");
		}
	}

}
