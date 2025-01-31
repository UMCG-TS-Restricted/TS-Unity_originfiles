using System;
using ADG;
using UnityEngine;


namespace insitu
{
	public class LatencyCheck : MonoBehaviour
	{
		[NonSerialized] public uint NexusId;
		[NonSerialized] public int SubjectIndex;
		[NonSerialized] public int MarkerIndex;
		[NonSerialized] public int ColorProperty;
		[NonSerialized] public double PreviousY;

		//public Library Library;
		public string Subject;
		public string Marker;
		public Material Material;
		public Color ColorFall;
		public Color ColorRise;


		/*public void Start()
		{
			ColorProperty = Shader.PropertyToID("_BaseColor");
		}

		public void Update()
		{
			var library = Library;
			var nexus = library.Nexus;
			if (NexusId != nexus.id)
				Initialize(nexus);
		}

		public void Initialize(Nexus nexus)
		{
			var subjects = nexus.subjects;
			if (subjects == null)
				return;

			var subject_index = -1;
			for (var i = 0; i < subjects.Length; i++)
			{
				var subject = subjects[i];
				if (string.IsNullOrEmpty(Subject) || Subject == subject.name)
				{
					subject_index = i;
					Subject = subject.name;
					break;
				}
			}

			var marker_index = -1;
			if (subject_index >= 0)
			{
				var subject = subjects[subject_index];
				var markers = subject.markers;
				for (var i = 0; i < markers.Length; i++)
				{
					var marker = markers[i];
					if (string.IsNullOrEmpty(Marker) || Marker == marker.name)
					{
						marker_index = i;
						Marker = marker.name;
						break;
					}
				}
			}

			SubjectIndex = subject_index;
			MarkerIndex = marker_index;
			NexusId = nexus.id;
		}

		public void LateUpdate()
		{
			var library = Library;
			var nexus = library.Nexus;
			var subjects = nexus.subjects;
			var subject_index = SubjectIndex;
			if (subjects == null || subject_index < 0 || subject_index >= subjects.Length)
				return;

			var subject = subjects[subject_index];
			var markers = subject.markers;
			var marker_index = MarkerIndex;
			if (markers == null || marker_index < 0 || marker_index >= markers.Length)
				return;

			var marker = markers[marker_index];
			//var position = marker.position.r3();
			var y = marker.position.z; // z is the y axis
			if (y < PreviousY)
			{
				Material.SetColor(ColorProperty, ColorFall);
			}
			else if (y > PreviousY)
			{
				Material.SetColor(ColorProperty, ColorRise);
			}

			PreviousY = y;
		}*/
	}
}
