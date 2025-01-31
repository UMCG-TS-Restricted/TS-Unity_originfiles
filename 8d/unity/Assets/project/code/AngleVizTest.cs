using insitu;
using TMPro;
using UnityEngine;


public class AngleVizTest : MonoBehaviour
{
	public App App;
	public Gizmo GizmoAngle;
	public Gizmo GizmoVector0;
	public Gizmo GizmoVector1;
	public TMP_Text Text;


	public int I0 = 0;
	public int I1 = 1;
	public int I2 = 2;

	public string Subject;
	public string C0;
	public string C1;
	public string C2;


	public void Update()
	{
		var worker = App.Worker;
		if (worker == null)
			return;


		Vector3 p0, p1, p2;
		var state = worker.State;
		var unlabeled = state.unlabeled;
		if (unlabeled.length > 2)
		{
			var u0 = unlabeled.At(I0);
			var u1 = unlabeled.At(I1);
			var u2 = unlabeled.At(I2);
			
			p0 = Vicon.ToUnityVector(u0.position).v3();
			p1 = Vicon.ToUnityVector(u1.position).v3();
			p2 = Vicon.ToUnityVector(u2.position).v3();
		}
		else
		{

			var subject_index = state.SubjectWith(Subject);
			if (subject_index < 0)
				return;

			var subject = state.subjects[subject_index];
			var m0 = state.MarkerOf(subject.markers, C0);
			var m1 = state.MarkerOf(subject.markers, C1);
			var m2 = state.MarkerOf(subject.markers, C2);
			if (m0 < 0 || m1 < 0 || m2 < 0)
				return;

			p0 = state.markers[m0].position.v3();
			p1 = state.markers[m1].position.v3();
			p2 = state.markers[m2].position.v3(); ;
		}


		var angle = GizmoAngle.Evaluate(p0, p1, p2, Vector3.up, 0.2f);
		GizmoVector0.Evaluate(p1, p0, Vector3.up);
		GizmoVector1.Evaluate(p1, p2, Vector3.up);
		Text.text = $"{angle * Mathf.Rad2Deg:0.0} deg";
		Text.transform.position = GizmoAngle.transform.position;
	}
}
