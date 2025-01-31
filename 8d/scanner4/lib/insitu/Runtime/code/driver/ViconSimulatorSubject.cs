using System;
using UnityEngine;


namespace insitu
{
	/// <summary>
	///		Mock object to simulate a subject.
	///		This can be used for testing when there is no Vicon host.
	/// </summary>
	/// <seealso cref="ViconSimulator"/>
	public class ViconSimulatorSubject : MonoBehaviour
	{
		[NonSerialized] public Vicon.Subject Subject;

		public Transform[] Segments;
		public Transform[] Markers;

		public string Name => gameObject.name;
	}
}
