#if UNITY_2022_1_OR_NEWER
using UnityEditor.Overlays;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Diagnostics;


namespace insitu
{
	[Overlay(typeof(SceneView), "insitu_toolbar_editor", "Telemetry", true, defaultDockZone = DockZone.BottomToolbar)]
	public class ToolbarEditor : Overlay, ICreateHorizontalToolbar
	{
		public Label Label;
		public Button OpenEditor;
		public Button SelectCurrent;
		public Button ToggleRecord;
		public Button Load;
		public Button Play;
		public Slider Slider;

		float PlayStart;
		Stopwatch PlayTimer;
		List<App> Apps;
		App Current;


		public static void FindAll<T>(List<T> list) where T : Object
		{
			list.Clear();
			var guids = AssetDatabase.FindAssets("t:"+typeof(T));
			for (var i = 0; i < guids.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				var asset = AssetDatabase.LoadAssetAtPath<T>(path);
				if (asset)
					list.Add(asset);
			}
		}

		public float DurationOf(App current)
		{
			var time = 0.0f;
			var files = current.LoadedFiles;
			for (var i = 0; i < files.length; i++)
			{
				var file = files[i];
				var frames = file.frames;
				if (frames.length < 1)
					continue;

				var first = frames[0];
				var last = frames.last;
				var delta = last.time - first.time;
				time += delta;
			}
			return time;
		}

		public float AlphaToTime(App current, float alpha)
		{
			var duration = DurationOf(current);
			return alpha * duration;
		}

		public float TimeToAlpha(App current, float time)
		{
			var duration = DurationOf(current);
			return time / duration;
		}
		
		public void OnUpdate(
			out string label,
			out bool select_visible,
			out string select_text,
			out bool record_visible,
			out bool record_enabled,
			out string record_text,
			out bool load_visible,
			out bool load_enabled,
			out string load_text,
			out bool play_visible,
			out string play_text,
			out bool slider_visible)
		{
			label = "Vicon data not found! Please create an insitu App asset";
			select_visible = false;
			select_text = string.Empty;
			record_visible = false;
			record_enabled = false;
			record_text = "Record";
			load_visible = false;
			load_enabled = false;
			load_text = "Load";
			play_visible = false;
			play_text = "Play";
			slider_visible = false;

			if (Apps == null || Apps.Count == 0)
			{
				Apps = new List<App>();
				FindAll(Apps);
			}

			App current = null;
			Vicon.Worker worker = null;
			for (var i = 0; i < Apps.Count; i++)
			{
				var app = Apps[i];
				if (!app) continue;

				if (current == null)
					current = app;

				if (app.Worker != null)
				{
					var w = app.Worker;
					if (Current == app)
					{
						current = app;
						worker = w;
						break;
					}
					else if (worker == null)
					{
						current = app;
						worker = w;
					}
				}
			}

			if (Current != current)
			{
				// TODO:
				Current = current;
			}


			if (!current)
				return;

			select_visible = true;
			select_text = current.name;
			label = "Vicon worker not started!";
			if (worker == null)
				return;


			record_visible = true;
			load_visible = true;


			var state = worker.State;
			if (state.version == 0)
			{
				label = "Waiting for worker..";
				return;
			}

			if (current.LoadedFiles.length > 0)
			{
				record_enabled = false;
				load_enabled = true;
				load_text = "Unload";
				play_visible = true;
				slider_visible = true;
				if (PlayTimer != null)
				{
					play_text = "Pause";
					var seconds = (float)PlayTimer.Elapsed.TotalSeconds;
					var time = seconds + AlphaToTime(current, PlayStart);
					var alpha = TimeToAlpha(current, time);
					Slider.SetValueWithoutNotify(alpha);
					current.PlaybackTime = time;
				}

				label = $"Showing frame at {current.PlaybackTime:0.0}";
				return;
			}


			switch (current.RecordingState)
			{
				default:
				case App.RecordingEnded:
				{
					record_enabled = state.version != 0;
					load_enabled = true;
					label = "";
				} break;
				case App.Recording:
				{
					record_text = "Stop";
					record_enabled = state.version != 0;
					load_enabled = true;
					label = "";
				} break;
				case App.RecordingEnding:
				{
					record_text = "Saving..";
					label = "";
				} break;
			}
		}

		public void OnUpdate()
		{
			OnUpdate(
				out var label,
				out var select_visible,
				out var select_text,
				out var record_visible,
				out var record_enabled,
				out var record_text,
				out var load_visible,
				out var load_enabled,
				out var load_text,
				out var play_visible,
				out var play_text,
				out var slider_visible);

			Label.text = label;
			SelectCurrent.style.maxWidth = select_visible ? 10000 : 0;
			SelectCurrent.visible = select_visible;
			SelectCurrent.text = select_text;
			ToggleRecord.style.maxWidth = record_visible ? 10000 : 0;
			ToggleRecord.visible = record_visible;
			ToggleRecord.SetEnabled(record_enabled);
			ToggleRecord.text = record_text;
			Load.style.maxWidth = load_visible ? 10000 : 0;
			Load.visible = load_visible;
			Load.SetEnabled(load_enabled);
			Load.text = load_text;
			Play.style.maxWidth = play_visible ? 10000 : 0;
			Play.visible = play_visible;
			Play.text = play_text;
			Slider.style.maxWidth = slider_visible ? 10000 : 0;
			Slider.visible = slider_visible;
		}

		public void Initialize(VisualElement root)
		{
			Label = new Label() { text = "" };

			OpenEditor = new Button { text = "Editor" };
			OpenEditor.clicked += () =>
			{
				var window = FileEditor.Open();
				window.App = Current;
			};

			SelectCurrent = new Button() { text = "Settings" };
			SelectCurrent.clicked += () =>
			{
				if (Current)
					Selection.activeObject = Current;

				OnUpdate();
			};

			ToggleRecord = new Button() { text = "Record" };
			ToggleRecord.clicked += () =>
			{
				var current = Current;
				if (!current) return;
				if (current.RecordingState == App.Recording)
				{
					// Stop recording
					var path = FileEditor.Save(current);
					current.RecordingState = App.RecordingEnding;
					OnUpdate();

					// Load created recording
					var file = Telemetry.File.Read(path);
					if (file.blocks.length == 0) return;

					current.LoadedFiles = current.LoadedFiles.Append(file);
				}
				else
				{
					// Start recording
					current.RecordingState = App.Recording;
					OnUpdate();
				}
			};

			Load = new Button() { text = "Save" };
			Load.clicked += () => FileEditor.Load(Current);

			Play = new Button() { text = "Play" };
			Play.clicked += () =>
			{
				if (PlayTimer == null)
				{
					PlayStart = Slider.value;
					PlayTimer = Stopwatch.StartNew();
				}
				else
				{
					PlayTimer = null;
				}
			};

			Slider = new Slider() { lowValue = 0, highValue = 1, };
			Slider.style.width = 100;
			Slider.RegisterValueChangedCallback(x =>
			{
				PlayTimer = null;

				var current = Current;
				if (current)
				{
					var alpha = Mathf.Clamp01(x.newValue);
					var time = AlphaToTime(current, alpha);
					current.PlaybackTime = time;
				}
			});

			root.Add(OpenEditor);
			root.Add(SelectCurrent);
			root.Add(ToggleRecord);
			root.Add(Load);
			root.Add(Play);
			root.Add(Slider);
			root.Add(Label);

			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
			OnUpdate();
		}

		public override VisualElement CreatePanelContent()
		{
			var root = new VisualElement() { name = "Recorder"};
			Initialize(root);
			return root;
		}

		public OverlayToolbar CreateHorizontalToolbarContent()
		{
			var root = new OverlayToolbar() { name = "Recorder" };
			Initialize(root);
			return root;
		}
	}
}
#else
#endif
