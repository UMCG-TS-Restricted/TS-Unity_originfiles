using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ADG;
using insitu.memory;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static insitu.Telemetry;
using static insitu.Vicon;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using UnityEngine.UIElements;


namespace insitu
{
	public sealed class Playback : MonoBehaviour
	{
		[NonSerialized] public byte[] Cache;
		[NonSerialized] public file File;
		[NonSerialized] public bool WasCall;
		[NonSerialized] public float StartTime;
		[NonSerialized] public float EndTime;
		[NonSerialized] public float TimerCurrent;
		[NonSerialized] public float TimerVelocity;

		public string Path;
		public bool Call;
		//[Range(0.0f, 1.0f)]
		public float Timer;


		public int Block;
		public int Frame0;
		public int Frame1;

		public TelemetryRigidbody[] Test;


		public void Awake()
		{
			Cache = new byte[Pool.page];
		}

		public void Update()
		{
			TimerCurrent = Mathf.SmoothDamp(TimerCurrent, Timer, ref TimerVelocity, 0.1f);
		}

		public unsafe void FixedUpdate()
		{
			if (Call)
			{
				Call = false;
				File = Deserialise(Path);
				var frames = File.frames;
				if (frames.length > 0)
				{
					var first = frames[0];
					var last = frames.last;
					StartTime = first.time;
					EndTime = last.time;
				}
			}


			if (File.frames.length > 0)
			{
				for (var i = 0; i < Test.Length; i++)
					Test[i].RequestRigidbody.isKinematic = true;

				var timer = TimerCurrent;// Mathf.LerpUnclamped(StartTime, EndTime, TimerCurrent);
				var file = File;
				var swap_endian = file.swap_endian;
				var blocks = file.blocks;
				var frames = file.frames;
				var entities = file.objects;

				DataAt(file, timer, out var fi0, out var fi1, out var alpha);
				var f0 = frames[fi0];
				var f1 = frames[fi1];
				var b = blocks[f0.block_index];
				var fb = frames[b.frame_index];
				var cache = Cache;

				Frame0 = fi0;
				Frame1 = fi1;
				Block = f0.block_index;

				for (var i = 0; i < fb.entity_length; i++)
				{
					var entity = entities[f1.entity_index + i];
					Telemetry.IDeserialize reference = null;
					for (var j = 0; j < Test.Length; j++)
					{
						if (Test[j].id == entity.id)
						{
							reference = Test[j];
							break;
						}
					}
					if (reference == null)
					{
						Debug.LogWarning($"Entity not found! ({entity.id})");
						continue;
					}

					// Prepare cache
					var cache_size = reference.Capacity;
					if (cache_size > cache.Length)
					{
						const int mask = Pool.page - 1;
						var capacity = (cache_size + mask) & ~mask;
						Cache = cache = new byte[capacity];
					}
					var cache_slice = new slice
					{
						data = cache,
						length = cache_size,
						offset = 0,
					};
					cache_slice.memset();

					var safety_count = 256;
					var ec = entity;
					do
					{
						var eptr = new SerializedBuffer
						{
							slice = new slice
							{
								data = file.data,
								offset = ec.data_index,
								length = ec.data_length,
							},
							swap_endian = swap_endian,
							version = ec.version,
						};
						reference.Deserialize(cache_slice, eptr, 0, 1);
						if (entity.next_index < 0)
							break;

						ec = entities[entity.next_index];
					} while (ec.frame_index <= fi0 && --safety_count > 0);

					// Last
					{
						var eptr = new SerializedBuffer
						{
							slice = new slice
							{
								data = file.data,
								offset = ec.data_index,
								length = ec.data_length,
							},
							swap_endian = swap_endian,
							version = ec.version,
						};
						reference.Deserialize(cache_slice, eptr, 1, alpha);
					}
				}
			}
		}

		public void OnDestroy()
		{
			
		}

		public struct obj
		{
			public ushort header;
			public byte flags;
			public byte version;
			public int id;
			public int child_count;
			public int data_index;
			public int data_length;
			public int previous_index;
			public int next_index;
			public int frame_index;

			public bool is_entity => (flags & FlagEntity) != 0;
			public bool is_group => (flags & FlagGroup) != 0;
			public bool has_body => (flags & FlagBody) != 0;
		}

		public struct entity
		{
			public ushort header;
			public byte flags;
			public byte version;
			public int id;
			public int data_index;
			public int data_length;
			public int previous_index;
			public int next_index;
			public int frame_index;
		}

		public struct frame
		{
			public int id;
			public float time;
			public int entity_index;
			public int entity_length;
			public int block_index;
		}

		public struct block
		{
			public int frame_index;
			public int frame_length;
		}

		public struct file
		{
			public byte[] data;
			public array<block> blocks;
			public array<frame> frames;
			public array<obj> objects;
			public bool swap_endian;
		}

		public static void DataAt(file file, float time, out int frame0, out int frame1, out float alpha)
		{
			var blocks = file.blocks;
			var frames = file.frames;
			frame0 = 0;
			frame1 = 0;
			alpha = 1.0f;
			for (var i = 0; i < blocks.length; i++)
			{
				var block = blocks[i];
				for (var j = 0; j < block.frame_length; j++)
				{
					var j_index = block.frame_index + j;
					var frame = frames[j_index];
					if (time < frame.time)
					{
						frame1 = j_index;
						alpha = Mathf.InverseLerp(frames[frame0].time, frames[frame1].time, time);
						return;
					}

					frame0 = j_index;
					frame1 = j_index;
				}
			}
		}

		public unsafe static file Deserialize(byte[] bytes, int start, int end)
		{
			var file = new file { data = bytes, };
			fixed (byte* start_ptr = bytes)
			{
				buffer.read(start_ptr + start, false, out ushort endianness_check);
				var e = endianness_check != FileDescriptor.Header;
				buffer.read(start_ptr + start, e, out endianness_check);
				if (endianness_check != FileDescriptor.Header)
				{
					Debug.LogError("Invalid file! File should start with the FileDescriptor Header");
					return file;
				}

				file.swap_endian = e;
				for (int i = start; i >= start && i < end;)
				{
					var obj = new obj { };
					var ptr = start_ptr + i;
					ptr = buffer.read(ptr, e, out obj.header);
					ptr = buffer.read(ptr, e, out obj.flags);
					ptr = buffer.read(ptr, e, out obj.version);
					if (obj.is_entity)
						ptr = buffer.read(ptr, e, out obj.id);
					if (obj.is_group)
						ptr = buffer.read(ptr, e, out obj.child_count);
					if (obj.has_body)
					{
						ptr = buffer.read(ptr, e, out obj.data_length);
						obj.data_index = (int)(ptr - start_ptr);
						ptr += obj.data_length;
					}

					i = (int)(ptr - start_ptr);
					file.objects = file.objects.Append(obj);
				}



				for (var i = 0; i < file.objects.length; i++)
				{
					var obj = file.objects[i];
					var header = obj.header;
					if ((header & 0xF000) == 0xE000)
					{
						Debug.Log("Entity");
						if (obj.id == 0) Debug.LogError("!!!1");
						var frame_index = file.frames.length - 1;
						var current_frame = file.frames.last;
						current_frame.entity_length++;
						file.frames.last = current_frame;

						for (var j = frame_index - 1; j >= 0; j--)
						{
							var frame = file.frames[j];
							for (var k = 0; k < frame.entity_length; k++)
							{
								var k_index = frame.entity_index + k;
								var other_entity = file.objects[k_index];
								if (other_entity.id == obj.id)
								{
									Debug.Assert(other_entity.next_index == -1);
									other_entity.next_index = i;
									file.objects[k_index] = other_entity;
									obj.previous_index = k_index;
									file.objects[i] = obj;
									goto __next;
								}
							}
						}

					__next:;
						if (obj.is_group)
							i += obj.child_count;
					}
					else switch (header)
					{
						case FileDescriptor.Header: { } break;

						case Metadata.Header:
						{
							if ((obj.flags & FlagTEXT) != 0)
								Debug.Log(Encoding.UTF8.GetString(file.data, obj.data_index, obj.data_length));
							if ((obj.flags & FlagJSON) != 0)
								Debug.Log(Encoding.UTF8.GetString(file.data, obj.data_index, obj.data_length));
						} break;

						case Telemetry.Block.Header:
						{
									Debug.Log("AAAAAAAAAAAAAAAAAAA");
							file.blocks = file.blocks.Append(new block
							{
								frame_index = file.frames.length,
								frame_length = 0,
							});
						} break;

						case Frame.Header:
						{
							int id;
							float time;
							fixed (byte* ptr = file.data)
							{
								buffer.read(ptr + obj.data_index + 0, e, out id);
								buffer.read(ptr + obj.data_index + 4, e, out time);
							}

							Debug.Log("frame");
							file.frames = file.frames.Append(new frame
							{
								id = id,
								time = time,
								entity_index = i + 1,
								entity_length = 0,
								block_index = file.blocks.length - 1,
							});

							var last = file.blocks.last;
							last.frame_length++;
							file.blocks.last = last;
						} break;

						case 0:
						{
							Debug.LogError("Failed to read value");
						} break;
						default:
						{
							Debug.LogWarning($"Unknown header {header:X4} at {i})");
						} break;
					}
				}
			}




			return file;
		}

		public static file Deserialise(string path)
		{
			var bytes = System.IO.File.ReadAllBytes(path);
			var result = Deserialize(bytes, 0, bytes.Length);

			Debug.Log("File summary:");
			Debug.Log($"Block count: {result.blocks.length}");
			for (var i = 0; i < result.blocks.length; i++)
				Debug.Log($"Block {i} starts at frame index {result.blocks[i].frame_index} and has {result.blocks[i].frame_length} frames");

			return result;
		}
	}
}