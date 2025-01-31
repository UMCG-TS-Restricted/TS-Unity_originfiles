using System;
using System.Drawing;
using System.IO;
using System.Text;
using ADG;
using insitu.memory;
using UnityEngine;
using Debug = UnityEngine.Debug;


/**
 * Goals:
 * - Store full world state (ignore equal to constructor value, and fixed values)
 * - Check every frame to last recorder world state, record new value if changed significantly
 * - Do a full world state record every so often
 * 
 * 
 * Objects should write to telemetry, telemetry should not query the objects.
 * There should probably be a full state buffer and a accumulative buffer.
 * When full write the full result.
 */
namespace insitu
{
	public sealed class Telemetry
	{
		/// <summary>
		///		Size of the data that every serialized object contains:
		///		[2 bytes - ushort] Type;
		///		[1 byte  - byte  ] Flags;
		///		[1 byte  - byte  ] Version;
		/// </summary>
		public const int SizeOfSuperHeader = 4;

		/// <summary>
		///		[4 bytes - int   ] ID;
		/// </summary>
		public const int SizeOfEntityHeader = 4;

		/// <summary>
		///		[2 bytes - ushort] Child Count > number of entities belonging to this entity (1 level deep)
		/// </summary>
		public const int SizeOfGroupHeader = 2;

		/// <summary>
		///		[4 bytes - int   ] Body size;
		/// </summary>
		public const int SizeOfBodyHeader = 4;



		public const int OffsetOfChildCount = 4;
		public const int OffsetOfSize = 12;
		public const int BlockFrameCapacity = 64;
		public const int FlagOneFrame = 1 << 0;
		public const int FlagCheckExistance = 1 << 1;

		public const byte FlagEntity = 1 << 1;
		public const byte FlagGroup  = 1 << 2;
		public const byte FlagBody   = 1 << 3;
		public const byte FlagTEXT   = 1 << 5;
		public const byte FlagJSON   = 1 << 6;
		public const byte FlagDebug  = 1 << 7;



		[NonSerialized] public Pool Cache;
		[NonSerialized] public array<Block > Blocks;
		[NonSerialized] public array<Entity> Entities;

		/// <summary>
		///		Return the object to its original state after constructing,
		///		but keeping the allocated memory.
		/// </summary>
		public void Clear()
		{
			Cache?.recycle();
			Entities.length = 0;
			Blocks.length = 0;
		}

		public bool Register(IEntity element, int flags = FlagCheckExistance)
		{
			var entities = Entities;
			var header = element.Header;
			Debug.Assert((header & 0xF000) == 0xE000);

			if ((flags & FlagCheckExistance) != 0)
			{
				for (var i = 0; i < entities.length; i++)
				{
					var entry = entities[i];
					if (entry.reference == element)
						return false;
				}
			}

			entities = entities.Append(new Entity
			{
				reference = element,
				flags = flags,
			});

			Entities = entities;
			return true;
		}

		public void AppendBlock()
		{
			Debug.Log("Appending new frame block!");
			var blocks = Blocks;
			blocks = blocks.Append();
			var block = blocks.last;
			if (block.frames == null)
				block.frames = Pool.create();
			else block.frames.recycle();
			block.frame_count = 0;
			blocks.last = block;
			Blocks = blocks;

			var entities = Entities;
			for (var i = 0; i < entities.length; i++)
			{
				var entity = entities[i];
				entity.cache = default;
				entities[i] = entity;
			}
			Entities = entities;

			var cache = Cache;
			if (cache != null)
				cache.recycle();
			Cache = cache;
		}

		public unsafe void MoveNext(int frame_id, float frame_time)
		{
			var new_block = false;
			if (Blocks.length == 0)
			{
				AppendBlock();
				new_block = true;
			}
			var blocks = Blocks;
			var block = blocks.last;
			if (block.frame_count >= BlockFrameCapacity)
			{
				AppendBlock();
				blocks = Blocks;
				block = blocks.last;
				new_block = true;
			}
			var frames = block.frames;

			// Write Frame
			const int frame_body_size = 8;
			const int frame_size = SizeOfSuperHeader + SizeOfBodyHeader + frame_body_size;
			var header_mem = frames.request(frame_size, true);
			fixed (byte* mem_ptr = header_mem.span)
			{
				// A frame is not a group as it is more of a flag marking all data after it of the frame
				var ptr = mem_ptr;
				ptr = buffer.write_header(ptr, Frame.Header, FlagBody, Frame.Version);
				ptr = buffer.write_body(ptr, frame_body_size);
				ptr = buffer.write(ptr, frame_id);
				ptr = buffer.write(ptr, frame_time);
			}

			var cache = Cache;
			if (cache == null)
			{
				cache = Pool.create();
				Cache = cache;
			}

			ushort child_count = 0;
			var entities = Entities;
			for (var i = 0; i < entities.length; i++)
			{
				var entity = entities[i];
				var reference = entity.reference;
				if (reference == null)
					continue; // TODO: Add destroy object event

				MoveNext(ref entity, frames, cache, new_block, false);
				entities[i] = entity;
				child_count++;
			}

			block.frame_count++;
			blocks.last = block;
			Blocks = blocks;

			// Cleanup
			for (var i = entities.length - 1; i >= 0; i--)
			{
				var entitiy = entities[i];
				if (entitiy.reference == null ||
					0 != (entitiy.flags & FlagOneFrame))
					entities = entities.Erase(i);
			}
			Entities = entities;
		}

		public static unsafe void MoveNext(ref Entity entity, Pool frames, Pool cache, bool new_frame, bool force)
		{
			const int align = 8 - 1;

			var reference = entity.reference;
			var cache_capacity = reference.Capacity;
			if (entity.cache.length < cache_capacity)
			{
				var allocate_size = (cache_capacity + align) & ~align;
				entity.cache = cache.request(allocate_size, true);
				new_frame = true;
			}

			if (new_frame)
				entity.cache.memset();

			// Write Entity
			byte flags = FlagBody;
			var header_size = SizeOfSuperHeader + SizeOfBodyHeader;

			var i_entity = reference as IEntity;
			if (i_entity != null)
			{
				flags |= FlagEntity;
				header_size += SizeOfEntityHeader;
			}

			ushort child_count = 0;
			var i_group = reference as IGroup;
			if (i_group != null)
			{
				flags |= FlagGroup;
				child_count = i_group.ChildCount;
				header_size += SizeOfGroupHeader;
			}

			var target = frames.request(header_size + reference.Capacity, false);
			fixed (byte* target_ptr = target.span)
			{
				var ptr = target_ptr;
				ptr = buffer.write_header(ptr, reference.Header, flags, reference.Version);
				if (i_entity != null)
					ptr = buffer.write_entity(ptr, i_entity.Identifier);
				if (i_group != null)
					ptr = buffer.write_group(ptr, child_count);
				ptr = buffer.write_body(ptr, target_ptr, out var body_offset);
				var body_length = reference.Serialize(entity.cache, ptr);
				if (body_length < 0)
				{
					if (!force)
						return;
					body_length = 0;
				}

				buffer.write_body(target_ptr + body_offset, body_length);
				var written = body_length + (int)(ptr - target_ptr);
				frames.request(written, true);
			}

			if (child_count > 0)
			{
				var capacity = (child_count + align) & ~align;
				if (entity.children == null)
					entity.children = new Entity[capacity];
				else if (entity.children.Length < child_count)
				{
					var new_buffer = new Entity[capacity];
					for (var i = 0; i < entity.children.Length; i++)
						new_buffer[i] = entity.children[i];
					entity.children = new_buffer;
				}

				for (int i = 0, il = child_count; i < il; i++)
				{
					var child = i_group.ChildAt(i);
					var child_entity = entity.children[i];
					if (child != child_entity.reference)
						child_entity.reference = child;

					MoveNext(ref child_entity, frames, cache, new_frame, true);
					entity.children[i] = child_entity;
				}
			}
		}

		public void Save(string path)
		{
			Write(path, Blocks);
		}

		public static unsafe void Write<T>(FileStream stream, byte[] bytes, T value) where T : unmanaged
		{
			fixed (byte * ptr = bytes)
			{
				buffer.write(ptr, value);
				stream.Write(bytes, 0, sizeof(T));
			}
		}

		public static unsafe void WriteHeader(FileStream stream, byte[] bytes, ushort header, byte flags, byte version)
		{
			Write(stream, bytes, header);
			Write(stream, bytes, flags);
			Write(stream, bytes, version);
		}

		public static unsafe void WriteBody(FileStream stream, byte[] bytes, int size)
		{
			Write(stream, bytes, size);
		}

		public static unsafe bool Write(string path, array<Block> blocks)
		{
			var descriptor_size = FileDescriptor.SizeOf(blocks);
			if (descriptor_size <= 64)
				return false;

			var info = new Json.Object
			{
				{"operatingSystem", SystemInfo.operatingSystem },
				{"processorType", SystemInfo.processorType },
				{"processorFrequency", SystemInfo.processorFrequency },
				{"processorCount", SystemInfo.processorCount },
				{"systemMemorySize", SystemInfo.systemMemorySize },
				{"deviceUniqueIdentifier", SystemInfo.deviceUniqueIdentifier },
				{"deviceName", SystemInfo.deviceName },
				{"deviceModel", SystemInfo.deviceModel },
				{"graphicsDeviceName", SystemInfo.graphicsDeviceName },
				{"graphicsDeviceVendor", SystemInfo.graphicsDeviceVendor },
				{"graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion },
				{"version", Application.version },
				{"unityVersion", Application.unityVersion },
				{"buildGUID", Application.buildGUID },
				{"identifier", Application.identifier },
				{"productName", Application.productName },
				{"companyName", Application.companyName },
				{"isEditor", Application.isEditor },
			}.ToString();
			var metadata_size = Metadata.SizeOf(info);
			if (metadata_size > Pool.page)
			{
				Debug.Log(info);
				Debug.LogError("An error occurred when trying to save telemetry data." +
				"The metadata info has too many characters - the system is unable to store objects larger than 4096 bytes." +
				"System metadata info is being reduced to a fixes string.");
				info = "{\"error\":\"Unable to store platform information.\"}";
				metadata_size = Metadata.SizeOf(info);
			}

			var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			stream.SetLength(descriptor_size + metadata_size);

			var bytes = new byte[Pool.page];
			FileDescriptor.Write(stream, bytes);
			Metadata.Write(stream, bytes, FlagJSON, info);
			for (var i = 0; i < blocks.length; i++)
				Block.Write(stream, bytes, blocks[i]);

			stream.Flush();
			stream.Close();
			Debug.Log("File written!");
			return true;
		}



		/// <summary>
		///		The serialized data of a file would be as follows:
		///		[n bytes - header] Header;
		/// </summary>
		public struct FileDescriptor
		{
			public const ushort Header = 0xF1DE;
			public const byte Version = 1;

			/// <summary>Size of a block in serialized bytes including the header data.</summary>
			public static int SizeOf(array<Block> blocks)
			{
				var size = SizeOfSuperHeader;
				for (var i = 0; i < blocks.length; i++)
					size += Block.SizeOf(blocks[i]);
				return size;
			}

			public static unsafe void Write(FileStream stream, byte[] bytes)
			{
				WriteHeader(stream, bytes, Header, 0, Version);
			}
		}

		/// <summary>
		///		The serialized data of metadata would be as follows:
		///		[8 bytes - header] Header;
		///		[4 bytes - int   ] Flags - can be used to determine how to interpret the data beyond;
		///		[n bytes - byte* ]
		/// </summary>
		public struct Metadata
		{
			public const ushort Header = 0x67AD;
			public const byte Version = 1;

			/// <summary>Size of a block in serialized bytes including the header data.</summary>
			public static int SizeOf(string value)
			{
				var size = SizeOfSuperHeader + SizeOfBodyHeader;
				size += Encoding.UTF8.GetByteCount(value);
				return size;
			}

			public static unsafe void Write(FileStream stream, byte[] bytes, byte flags, string value)
			{
				WriteHeader(stream, bytes, Header, (byte)(flags | FlagBody), Version);
				var length = Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 16);
				WriteBody(stream, bytes, length);
				stream.Write(bytes, 16, length);
			}
		}

		/// <summary>
		///		The serialized data of a block would be as follows:
		///		[8 bytes - header] Header;
		///		[n bytes - Frame*]
		/// </summary>
		public struct Block
		{
			public const ushort Header = 0xB10C;
			public const byte Version = 1;
			public ushort frame_count;
			public Pool frames;

			/// <summary>Size of a block in serialized bytes including the header data.</summary>
			public static int SizeOf(Block block)
			{
				var size = SizeOfSuperHeader;
				var frames = block.frames;
				if (frames != null && frames.head != null)
				{
					var current = frames.tail;
					while (current != null)
					{
						size += current.length;
						if (current == frames.head) break;
						current = current.next;
					}
				}
				return size;
			}

			public static unsafe void Write(FileStream stream, byte[] bytes, Block block)
			{
				var size = SizeOf(block);
				WriteHeader(stream, bytes, Header, 0, Version);
				Debug.Log($"Writing block with {block.frame_count} frames and a size of {size}");
				var frames = block.frames;
				if (frames != null && frames.head != null)
				{
					var current = frames.tail;
					while (current != null)
					{
						stream.Write(current.data, 0, current.length);
						if (current == frames.head)
							break;

						current = current.next;
					}
				}
			}
		}

		/// <summary>
		///		The serialized data of a frame would be as follows:
		///		[n bytes - header ] Header;
		///		[n bytes - body   ] Body Header;
		///		[4 bytes - int    ] Frame index;
		///		[4 bytes - float  ] Timestamp in seconds;
		///		[n bytes - Entity*]
		/// </summary>
		public struct Frame
		{
			public const ushort Header = 0xF47E;
			public const byte Version = 1;
		}

		/// <summary>
		///		The serialized data of an entity would be as follows:
		///		[n bytes - header] Header;
		///		[n bytes - ?     ] Data retrieved from ITelemetry custom to each type of entity;
		/// </summary>
		public struct Entity
		{
			public IObject reference;
			public int flags;
			public slice cache;
			public Entity[] children;
		}

		public interface ICache
		{
			public int Capacity { get; }
		}

		public interface IObject : ICache
		{
			/// <summary>
			///		The header is a unique identifier to determine the type of the data.
			///		It not only has to be unique in its value, but also when reversing its endianness not should it be a palindrome.
			/// </summary>
			/// <example>
			///		58597 (0xE4E5) is valid.
			///		58389 (0xE415) is valid.
			///		58389 (0xE4EE) is valid.
			///		46308 (0xB4E4) is invalid, because the first 4 bits should be 14 (0b1110 - 0xE).
			///		58596 (0xE4E4) is invalid, because swapping its endianess will have the same value.
			///		58852 (0xE5E4) will be invalid if 58852 (0xE4E5) is already taken.
			///	</example>
			public ushort Header { get; }

			/// <summary>
			///		Serialization version used to support backwards compatibility
			/// </summary>
			public byte Version { get; }


			/// <summary>
			///		Serialize the instance to a buffer.
			/// </summary>
			/// <param name="cache">The previous stored accumulative buffer of the instance used to check if changes have occurred</param>
			/// <param name="destination">Destination buffer, this can be a null value</param>
			/// <returns>The size in bytes of the serialized instance</returns>
			public unsafe int Serialize(slice cache, byte* destination);
		}

		public interface IEntity : IObject
		{
			public int Identifier { get; }
		}

		public interface IGroup
		{
			public ushort ChildCount { get; }
			public IObject ChildAt(int index);
		}


		public interface IDeserialize : ICache
		{
			public bool CanDeserialize(ushort header);
			public unsafe void Deserialize(slice cache, SerializedBuffer next, int flags, float alpha);
		}
	}
}