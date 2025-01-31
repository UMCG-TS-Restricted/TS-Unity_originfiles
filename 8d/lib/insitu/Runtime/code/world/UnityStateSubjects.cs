using System;
using System.Collections.Generic;
using UnityEngine;


namespace insitu
{
	public class UnityStateSubjects : MonoBehaviour
	{
		[NonSerialized] public int Version;
		[NonSerialized] public List<UnityStateSubject> Subjects;

		public App App;

		public void Update()
		{
			if (!App.FetchState(out var state))
				return;

			if (Subjects == null)
				Subjects = new List<UnityStateSubject>();

			var version = state.version;
			if (version != Version)
			{
				Version = version;
				Unity.Clear(Subjects);

				var subjects = state.subjects;
				for (var i = 0; i < subjects.length; i++)
				{
					var subject = subjects[i];
					var obj = new GameObject(subject.name);
					var child = obj.AddComponent<UnityStateSubject>();
					child.App = App;
					obj.transform.SetParent(transform, false);
					Subjects.Add(child);
				}
			}
		}
	}
}
