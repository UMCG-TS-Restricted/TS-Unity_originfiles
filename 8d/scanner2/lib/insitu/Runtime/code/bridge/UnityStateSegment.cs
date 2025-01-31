using System;
using System.Collections.Generic;
using ADG;
using UnityEngine;
using static insitu.Vicon;


namespace insitu
{
	/// <summary>
	///		Segment reference to Vicon segment.
	/// </summary>
	public class UnityStateSegment : PoseBehaviour, IPoseSource
	{
		/// <summary>
		///		Required successful marker reads in order to be eligable to create a reference point.
		///		0 means all markers must be succesful.
		///		If this number is higher than the amount of markers inside the segment, the reference point will never be made.
		/// </summary>
		public const int RequiredSuccesses = 0;

		[NonSerialized] public int Index;
		[NonSerialized] public int Version;
		[NonSerialized] public string CachedName;
		[NonSerialized] public string CachedSubject;
		[NonSerialized] public List<UnityStateMarker> Markers;
		[NonSerialized] public List<UnityStateSegment> Segments;
		[NonSerialized] public array<double4> Reference;
		[NonSerialized] public Segment Segment;
		[NonSerialized] public pose UnityPose;

		public App App;
		public StringReference Subject;
		public string Name;
		public bool SavedReference;

		public void Awake()
		{
			Segments = new List<UnityStateSegment>();
			Markers = new List<UnityStateMarker>();
			UnityPose = pose.identity;
		}

		public void ApplyCurrent()
		{
			transform.SetPositionAndRotation(
				UnityPose.position.v3(),
				UnityPose.rotation.q());

			var markers = Markers;
			for (var i = 0; i < markers.Count; i++)
				markers[i].ApplyCurrent();

			var segments = Segments;
			for (var i = 0; i < segments.Count; i++)
				segments[i].ApplyCurrent();
		}

		/// <summary>
		///		Rebuild children.
		/// </summary>
		public bool Scan(State state)
		{
			var name = Name;
			var subject_name = Subject.Value;

			Version = state.version;
			CachedName = name;
			CachedSubject = subject_name;
			Util.Clear(Markers);
			Util.Clear(Segments);

			var subject_index = state.SubjectWith(subject_name);
			if (subject_index < 0)
			{
				Index = -1;
				return false;
			}

			var subject = state.subjects[subject_index];
			var segment_index = state.SegmentWith(subject.segments, name);
			Index = segment_index;

			var segments = subject.segments;
			for (var i = 0; i < segments.length; i++)
			{
				var segment = state.segments[segments.offset + i];
				if (segment.parent == name)
				{
					var obj = new GameObject(segment.name);
					var child = obj.AddComponent<UnityStateSegment>();
					child.enabled = false;
					child.App = App;
					child.Subject = Subject;
					child.Name = segment.name;
					obj.transform.SetParent(transform, false);
					Segments.Add(child);
				}
			}

			var markers = subject.markers;
			for (var i = 0; i < markers.length; i++)
			{
				var marker = state.markers[markers.offset + i];
				if (marker.parent == name)
				{
					var obj = new GameObject(marker.name);
					var child = obj.AddComponent<UnityStateMarker>();
					child.enabled = false;
					child.App = App;
					child.Subject = Subject;
					child.Name = marker.name;
					obj.transform.SetParent(transform, false);
					Markers.Add(child);
				}
			}

			return true;
		}

		public bool StoreReference(Json.Object settings)
		{
			if (settings == null)
				return false;

			if (Reference.length <= 0)
				return false;

			var id = $"{CachedSubject}:{CachedName}";
			var segments = settings.EnsuredObjectOf("segments");
			var data = segments.EnsuredObjectOf(id);

			var array = new Json.Array();
			for (var i = 0; i < Reference.length; i++)
			{
				var reference = Reference[i];
				array.Add(reference.x);
				array.Add(reference.y);
				array.Add(reference.z);
				array.Add(reference.w);
			}

			data["reference"] = array;
			return true;
		}

		public bool LoadReference(Json.Object settings, out bool exists)
		{
			exists = false;
			if (settings == null)
				return false;

			var id = $"{CachedSubject}:{CachedName}";
			var segments = settings.EnsuredObjectOf("segments");
			var data = segments.ObjectOf(id);
			if (data == null)
				return false;

			var markers = Markers;
			var marker_count = markers.Count;

			exists = true;
			var reference = data.ArrayOf("reference");
			if (reference == null || reference.Count != marker_count * 4)
				return false;

			Reference = new array<double4>(marker_count, marker_count);
			for (var i = 0; i < marker_count; i++)
				Reference[i] = double4.from(reference, i * 4, default);

			return true;
		}

		public bool CreateReference(int required_successes)
		{
			var markers = Markers;
			var marker_count = markers.Count;
			if (marker_count < 3)
				return false;

			if (required_successes <= 0)
				required_successes = marker_count;

			for (var i = 0; i < marker_count; i++)
			{
				var marker = markers[i].Marker;
				if (marker.valid != 0)
					required_successes--;
			}

			if (required_successes <= 0)
			{
				Reference = new array<double4>(marker_count, marker_count);
				for (var i = 0; i < marker_count; i++)
				{
					var marker = markers[i].Marker;
					var reference = marker.vicon_position.d4(marker.valid);
					Reference[i] = reference;
				}

				return true;
			}

			return false;
		}

		public bool Fetch() => App.FetchState(App, out var state) && Fetch(App, state);
		public bool Fetch(App app, State state)
		{
			var name = Name;
			var subject_name = Subject.Value;
			if (state.version != Version || name != CachedName || subject_name != CachedSubject)
				Scan(state);

			if (Index < 0)
				return false;

			var segment = Segment = state.segments[Index];

			var markers = Markers;
			var marker_count = markers.Count;
			for (var i = 0; i < marker_count; i++)
				markers[i].Fetch(state);

			for (var i = 0; i < Segments.Count; i++)
				Segments[i].Fetch(app, state);

			var valid_rotation = (byte)0;
			var valid_position = (byte)0;
			var current_rotation = double4.identity;
			var current_centroid = new double3 { };
			if (segment.valid_rotation == 0)
			{
				if (Reference.length == 0)
				{
					var success = false;
					var save = SavedReference;
					if (SavedReference)
					{
						var settings = app.Settings;
						if (LoadReference(settings, out var exists))
							Debug.Log($"Loaded {CachedSubject}:{CachedName} reference successfully.");

						if (exists)
							save = false;
					}

					if (!success && CreateReference(RequiredSuccesses) && save)
					{
						var settings = app.FetchSettings();
						if (StoreReference(settings))
						{
							app.Save();
							Debug.Log("Succesfully saved new reference point");
						}
					}
				}

				if (Reference.length > 0)
				{
					var reference_centroid = new double3 { };
					{
						var valid_vertices = 0;
						for (var i = 0; i < marker_count; i++)
						{
							var marker = markers[i].Marker;
							if (Reference[i].w < 1 || marker.valid == 0)
								continue;

							reference_centroid += Reference[i].d3();
							current_centroid += marker.vicon_position;
							valid_vertices++;
						}

						if (valid_vertices > 0)
						{
							reference_centroid /= valid_vertices;
							current_centroid /= valid_vertices;
						}
					}

					// Compute correlation
					var correlation = new double3x3 { };
					for (var i = 0; i < marker_count; i++)
					{
						var marker = markers[i].Marker;
						if (Reference[i].w < 1 || marker.valid == 0)
							continue;

						var r = Reference[i];
						var c = marker.vicon_position;
						var ri = new double3
						{
							x = r.x - reference_centroid.x,
							y = r.y - reference_centroid.y,
							z = r.z - reference_centroid.z,
						};
						var ci = new double3
						{
							x = c.x - current_centroid.x,
							y = c.y - current_centroid.y,
							z = c.z - current_centroid.z,
						};
						correlation = correlation + double3x3.outer(ri, ci);
					}

					InsituDLL.svd(ref correlation, out var m);
					current_rotation = double3x3.rotation(m);
					valid_rotation = 1;
					valid_position = 1;
				}
			}
			else
			{
				current_rotation = segment.vicon_rotation;
				valid_rotation = 1;
			}


			if (segment.valid_position != 0)
			{
				current_centroid = segment.vicon_position;
				valid_position = 1;
			}

			current_centroid = double4x4.mul(state.position_transform, current_centroid, 1.0);
			UnityPose = new pose
			{
				position = current_centroid,
				valid_position = valid_position,
				rotation = current_rotation,
				valid_rotation = valid_rotation,
			};
			return true;
		}

		public override pose Pose()
		{
			if (!enabled)
				return UnityPose;

			if (Fetch())
				return UnityPose;

			return new pose
			{
				position = double3.from(transform.position),
				valid_position = 0,
				rotation = double4.from(transform.rotation),
				valid_rotation = 0,
			};
		}

		public pose PoseSource() => Fetch() ? Segment.vicon_pose : pose.identity;

		public void Update()
		{
			if (Fetch())
				ApplyCurrent();
		}

		public void OnDrawGizmosSelected()
		{
			var pose = Pose();
			if (pose.valid_position == 0)
				return;

			var position = pose.position.v3();
			if (pose.valid_rotation != 0)
			{
				var rotation = pose.rotation.q();
				var matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
				Gizmos.matrix = matrix;
				position = Vector3.zero;
			}

			Gizmos.color = Color.yellow;
			Gizmos.DrawCube(position, Vector3.one * 0.2f);
		}
	}
}
