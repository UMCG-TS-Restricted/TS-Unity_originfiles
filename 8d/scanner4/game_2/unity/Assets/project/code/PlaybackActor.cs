using System;
using System.Collections.Generic;
using ADG;
using insitu;
using UnityEngine;


public class PlaybackActor : MonoBehaviour
{
	public const ushort TypeId = 0xEAAC;
	[NonSerialized] public int Id;
	[NonSerialized] public Data Cache;

	public App App;
	public Transform Body;
	public Transform LeftHand;
	public Transform LeftElbow;
	public Transform LeftShoulder;
	public Transform RightHand;
	public Transform RightElbow;
	public Transform RightShoulder;
	public Transform LeftFoot;
	public Transform LeftKnee;
	public Transform LeftHip;
	public Transform RightFoot;
	public Transform RightKnee;
	public Transform RightHip;

	public struct Data
	{
		public Vector3 Body;
		public Vector3 LeftHand;
		public Vector3 LeftElbow;
		public Vector3 LeftShoulder;
		public Vector3 RightHand;
		public Vector3 RightElbow;
		public Vector3 RightShoulder;
		public Vector3 LeftFoot;
		public Vector3 LeftKnee;
		public Vector3 LeftHip;
		public Vector3 RightFoot;
		public Vector3 RightKnee;
		public Vector3 RightHip;

		public static Json.Array json(Vector3 v) => new Json.Array { v.x, v.y, v.z };

		public Json.Object json(int id) => new Json.Object
		{
			{"header", TypeId },
			{"type", "actor" },
			{"id", id },
			{"body", json(Body) },
			{"left_hand", json(LeftHand) },
			{"left_elbow", json(LeftElbow) },
			{"left_shoulder", json(LeftShoulder) },
			{"right_hand", json(RightHand) },
			{"right_elbow", json(RightElbow) },
			{"right_shoulder", json(RightShoulder) },
			{"left_foot", json(LeftFoot) },
			{"left_knee", json(LeftKnee) },
			{"left_hip", json(LeftHip) },
			{"right_foot", json(RightFoot) },
			{"right_knee", json(RightKnee) },
			{"right_hip", json(RightHip) },
		};
	}

	public void Write(Telemetry telemetry)
	{
		if (Id == 0)
			Id = telemetry.EntityId();

		telemetry.Begin(TypeId, Telemetry.FlagBody | Telemetry.FlagEntity, 1, id: Id);
		telemetry.Write(Body.localPosition);
		telemetry.Write(LeftHand.localPosition);
		telemetry.Write(LeftElbow.localPosition);
		telemetry.Write(LeftShoulder.localPosition);
		telemetry.Write(RightHand.localPosition);
		telemetry.Write(RightElbow.localPosition);
		telemetry.Write(RightShoulder.localPosition);
		telemetry.Write(LeftFoot.localPosition);
		telemetry.Write(LeftKnee.localPosition);
		telemetry.Write(LeftHip.localPosition);
		telemetry.Write(RightFoot.localPosition);
		telemetry.Write(RightKnee.localPosition);
		telemetry.Write(RightHip.localPosition);
		telemetry.End();
	}

	public static bool Read(insitu.telemetry.Object obj, out Data data)
	{
		data = default;
		if (obj.type != TypeId)
			return false;

		var reader = obj.Read(default);
		reader = reader.read(out data.Body);
		reader = reader.read(out data.LeftHand);
		reader = reader.read(out data.LeftElbow);
		reader = reader.read(out data.LeftShoulder);
		reader = reader.read(out data.RightHand);
		reader = reader.read(out data.RightElbow);
		reader = reader.read(out data.RightShoulder);
		reader = reader.read(out data.LeftFoot);
		reader = reader.read(out data.LeftKnee);
		reader = reader.read(out data.LeftHip);
		reader = reader.read(out data.RightFoot);
		reader = reader.read(out data.RightKnee);
		reader = reader.read(out data.RightHip);
		return true;
	}

	public void Apply(Data data)
	{
		Body.localPosition = data.Body;
		LeftHand.localPosition = data.LeftHand;
		LeftElbow.localPosition = data.LeftElbow;
		LeftShoulder.localPosition = data.LeftShoulder;
		RightHand.localPosition = data.RightHand;
		RightElbow.localPosition = data.RightElbow;
		RightShoulder.localPosition = data.RightShoulder;
		LeftFoot.localPosition = data.LeftFoot;
		LeftKnee.localPosition = data.LeftKnee;
		LeftHip.localPosition = data.LeftHip;
		RightFoot.localPosition = data.RightFoot;
		RightKnee.localPosition = data.RightKnee;
		RightHip.localPosition = data.RightHip;
	}

	public void Update()
	{
		if (!App)
			return;

		var actors = App.Actors;
		if (actors == null)
			App.Actors = actors = new List<PlaybackActor>();

		if (!actors.Contains(this))
			actors.Add(this);
	}

	public void OnDisable()
	{
		if (!App)
			return;

		var actors = App.Actors;
		if (actors != null)
			actors.Remove(this);
	}
}
