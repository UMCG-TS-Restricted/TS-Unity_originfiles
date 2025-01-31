using System;
using UnityEngine;


namespace insitu
{
	public class ViconSimulatorSubject : MonoBehaviour
	{
		[NonSerialized] public Vicon.Subject Subject;

		public Transform[] Segments;
		public Transform[] Markers;

		public string Name => gameObject.name;
	}
}
