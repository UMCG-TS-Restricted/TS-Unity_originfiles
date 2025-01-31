using System;
using System.Collections.Generic;
using System.IO;
using insitu;
using UnityEngine;

public class test_telemetry_main : MonoBehaviour
{
	[NonSerialized] public bool WasRecord;

	public Telemetry Telemetry;
	public TelemetryRigidbody[] TelemetryRigidbody;
	public bool Record;


	public void Start()
	{
		Telemetry = new Telemetry();
	}

	public void FixedUpdate()
	{
		if (Record)
		{
			if (!WasRecord)
			{
				Telemetry.Clear();
				for (var i = 0; i < TelemetryRigidbody.Length; i++)
					Telemetry.Register(TelemetryRigidbody[i]);
				WasRecord = Record;
			}

			Telemetry.MoveNext(Time.frameCount, Time.time);
		}
		else if (WasRecord)
		{
			Telemetry.Save("test.dat");
			WasRecord = false;
		}
	}
}
