using ADG;
using UnityEngine;


namespace insitu.telemetry
{
	/// <summary>
	///		File data
	/// </summary>
	public struct File
	{
		public const ushort TypeId = 0xF1DE;
		public const byte Version = 1;

		public byte[] data;
		public Json.Object metadata;
		public array<TypeInfo> types;
		public array<string> cached_strings;
		public array<range> blocks;
		public array<Frame> frames;
		public array<Object> objects;

		/// <summary>
		///		If true, the endianness of the file and the current machine are different.
		///		This means the data has to be reorientated to be valid.
		/// </summary>
		public bool swap_endian;

		public TypeInfo TypeOf(ushort type)
		{
			for (var i = 0; i < types.length; i++)
			{
				var element = types[i];
				if (element.type == type)
					return element;
			}
			return null;
		}

		/// <summary>
		///		Find the frame before and after <paramref name="time"/>.
		/// </summary>
		public void DataAt(float time, out int frame0, out int frame1, out float alpha)
		{
			frame0 = 0;
			frame1 = 0;
			alpha = 1.0f;
			for (var i = 0; i < blocks.length; i++)
			{
				var block = blocks[i];
				for (var j = 0; j < block.length; j++)
				{
					var j_index = block.offset + j;
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

		public int FrameAt(float time)
		{
			if (time < frames[0].time)
				return 0;

			for (var i = 1; i < frames.length; i++)
			{
				var frame = frames[i];
				if (time < frame.time)
					return i - 1;
			}

			return frames.length - 1;
		}

		/// <summary>
		///		Convert the read bytes in a file to a malleable representation.
		/// </summary>
		public static File Read(slice<byte> data)
		{
			var file = new File { data = data.elements, };
			var reader = new FileReader
			{
				data = data,
				string_cache = default,
				swap_endianness = false,
			};

			// Detect endianness
			{
				ushort file_type_id;
				reader.read(out file_type_id);
				reader.swap_endianness = file_type_id != TypeId;
				reader.read(out file_type_id);
				if (file_type_id != TypeId)
				{
					Debug.LogError(error.InvalidFileTypeId);
					return file;
				}
			}

			// Create all objects
			while (reader.data.length >= 4)
			{
				var obj = new Object
				{
					next_index = -1,
					previous_index = -1,
				};

				reader = reader.read(out obj.type);
				reader = reader.read(out obj.flags);
				reader = reader.read(out obj.version);

				if (obj.is_entity)
					reader = reader.read(out obj.entity);

				if (obj.has_body)
				{
					reader = reader.read(out int data_length);
					obj.data = new slice<byte>
					{
						elements = data.elements,
						offset = reader.data.offset,
						length = data_length,
					};

					reader.data.offset += data_length;
					reader.data.length -= data_length;
				}

				file.objects = file.objects.Append(obj);
			}

			// Parse primary types
			array<FieldInfo> fields = default;
			for (var i = 0; i < file.objects.length; i++)
			{
				var obj = file.objects[i];
				if (JsonData.Read(obj, out var json_data))
				{
					if (json_data == null)
					{
						Debug.LogError(error.JsonParseFailed);
					}
					else
					{
						if (file.metadata != null)
							Debug.LogWarning(error.MetadataChange(file.metadata, json_data));
						file.metadata = json_data;
					}
				}
				else if (CachedString.Read(obj, out var cached_string))
				{
					file.cached_strings = file.cached_strings.Append(cached_string);
				}
				else if (FieldInfo.Read(obj, out var field_info))
				{
					fields = fields.Append(field_info);
				}
				else if (TypeInfo.Read(obj, fields, out var type_info))
				{
					file.types = file.types.Append(type_info);
				}
				else if (Block.Read(obj))
				{
					var block = new range
					{
						offset = file.frames.length,
						length = 0,
					};

					file.blocks = file.blocks.Append(block);
				}
				else if (Frame.Read(obj, out var frame))
				{
					var block_index = file.blocks.length - 1;
					frame.block_index = block_index;
					frame.children.offset = i + 1;
					file.frames = file.frames.Append(frame);

					var block = file.blocks[block_index];
					block.length++;
					file.blocks[block_index] = block;
				}

				obj.self_index = i;
				obj.frame_index = file.frames.length - 1;
				file.objects[i] = obj;
			}
			
			// Determine frame lengths
			for (var i = 0; i < file.frames.length - 1; i++)
			{
				var frame = file.frames[i];
				var frame_start = frame.children.offset;
				var next = file.frames[i + 1];
				var next_start = next.children.offset;
				frame.children.length = next_start - frame_start - 1;
				file.frames[i] = frame;
			}
				
			// Link objects to primary data
			for (var i = 0; i < file.objects.length; i++)
			{
				var obj = file.objects[i];
				if ((obj.flags & Telemetry.FlagEntity) != 0)
				{
					Debug.Assert(obj.entity != 0);
					var frame_index = obj.frame_index;
					for (var j = frame_index - 1; j >= 0; j--)
					{
						var frame = file.frames[j];
						for (var k = 0; k < frame.children.length; k++)
						{
							var k_index = frame.children.offset + k;
							var other_entity = file.objects[k_index];
							if (other_entity.entity == obj.entity)
							{
								//Debug.Assert(other_entity.next_index == -1);
								other_entity.next_index = i;
								file.objects[k_index] = other_entity;
								obj.previous_index = k_index;
								goto __next;
							}
						}
					}
				}

				__next:;
				file.objects[i] = obj;
			}

			return file;
		}

		/// <summary>
		///		Read file at <paramref name="path"/>.
		/// </summary>
		public static File Read(string path)
		{
			if (!System.IO.File.Exists(path))
				return default;

			var bytes = System.IO.File.ReadAllBytes(path);
			var result = Read(new slice<byte>
			{
				elements = bytes,
				offset = 0,
				length = bytes.Length,
			});

			Debug.Log("File summary:");
			Debug.Log($"Block count: {result.blocks.length}");
			for (var i = 0; i < result.blocks.length; i++)
				Debug.Log($"Block {i} starts at frame index {result.blocks[i].offset} and has {result.blocks[i].length} frames");

			return result;
		}
	}
}
