using UnityEngine;


namespace insitu
{
	public class UnityState : MonoBehaviour
	{
		public App App;
		
		public void Start()
		{
			// Devices
			{
				var obj = new GameObject("devices");
				var child = obj.AddComponent<UnityStateDevices>();
				child.App = App;
				obj.transform.SetParent(transform, false);
			}
			
			// Subjects
			{
				var obj = new GameObject("subjects");
				var child = obj.AddComponent<UnityStateSubjects>();
				child.App = App;
				obj.transform.SetParent(transform, false);
			}
		}

		public void Update()
		{
			if (!App.FetchState(out var state))
				return;

			gameObject.name = "state v" + state.version + " - frame: " + state.frame;
		}
	}
}
