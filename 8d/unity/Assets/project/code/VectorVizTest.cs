using System.Collections;
using System.Collections.Generic;
using insitu;
using TMPro;
using UnityEngine;
using static insitu.Vicon;

public class VectorVizTest : MonoBehaviour
{
	public App App;
	public Gizmo GizmoVector;
	public TMP_Text Text;
	public string Device;

	public void Update()
	{
		var worker = App.Worker;
		if (worker == null)
			return;

		var state = worker.State;
		var plates = state.plates;
		if (plates.length <= 0)
			return;

		var plate = plates[0];
		var origin = transform.position;
		GizmoVector.Evaluate(origin, origin + plate.force.v3(), Vector3.up);
		Text.text = $"{plate.force.v3().magnitude:0.0} N";
		Text.transform.position = origin + plate.force.v3() * 0.1f;
	}
}
