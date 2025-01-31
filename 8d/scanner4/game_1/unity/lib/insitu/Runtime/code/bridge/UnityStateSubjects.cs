using System;
using System.Collections.Generic;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		A collection of UnityStateSubject.
	///		This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime.
	///		Use UnityStateSubject directly to make references in the editor.
	/// </summary>
	public class UnityStateSubjects : MonoBehaviour
	{
		[NonSerialized] public int Version;
		[NonSerialized] public List<UnityStateSubject> Subjects;

		[Note("A collection of UnityStateSubject. This is mostly used for debug purposes to read the Vicon state as references to its children can only be made in runtime. Use UnityStateSubject directly to make references in the editor.")]
		public App App;

		public void Awake()
		{
			Subjects = new List<UnityStateSubject>();
		}

		public void ApplyCurrent()
		{
			var subjects = Subjects;
			for (var i = 0; i < subjects.Count; i++)
			{
				var subject = subjects[i];
				subject.ApplyCurrent();
			}
		}
			
		/// <summary>
		///		Rebuild children
		/// </summary>
		public bool Scan(State state)
		{
			Version = state.version;
			Util.Clear(Subjects);

			var subjects = state.subjects;
			for (var i = 0; i < subjects.length; i++)
			{
				var subject = subjects[i];
				var obj = new GameObject(subject.name);
				var child = obj.AddComponent<UnityStateSubject>();
				child.enabled = false;
				child.App = App;
				child.Name = subject.name;
				obj.transform.SetParent(transform, false);
				Subjects.Add(child);
			}

			return true;
		}

		/// <summary>
		///		Gather Vicon data
		/// </summary>
		public bool Fetch() => App.FetchState(App, out var state) && Fetch(App, state);

		public bool Fetch(App app, State state)
		{
			if (state.version != Version)
				Scan(state);

			var subjects = Subjects;
			for (var i = 0; i < subjects.Count; i++)
			{
				var subject = subjects[i];
				subject.Fetch(app, state);
			}

			return true;
		}

		public void Update()
		{
			if (Fetch())
				ApplyCurrent();
		}
	}
}
