using System;
using UnityEngine;


namespace insitu
{
	public class MutateTranslationFactor : PoseBehaviour
	{
		[NonSerialized] public pose Reference;

		public App App;
		public PoseBehaviour Input;
		public float Factor;
		[Tooltip("When true Pose will return the value directly provider by Input")]
		public bool Bypass;

		[ContextMenu("Create Reference")]
		public void CreateReference()
		{
			Reference = Input.Pose();
		}

		[ContextMenu("Store Reference")]
		public void StoreReference()
		{
			var settings = App.FetchSettings();
			settings["MutateTranslationFactor:" + gameObject.name] = Reference.json();
			App.Save(settings);
		}

		public override pose Pose()
		{
			var pose = Input.Pose();
			if (Bypass)
				return pose;

			var position = pose.position;
			var delta = position - Reference.position;
			var movement = delta * Factor;
			pose.position = Reference.position + movement;
			return pose;
		}
	}
}
