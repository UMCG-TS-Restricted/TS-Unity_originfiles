using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using ADG;
using insitu.memory;
using insitu.telemetry;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace insitu
{
	/// <remarks>
	///		The possibility of using reflection (attributes) has been considered.
	///		This has not been done as it limits the target platforms; IL2CPP builds cannot be used otherwise.
	/// </remarks>
	public sealed class Telemetry
	{
		/// <summary>
		///		The amount of frames a single block contains
		/// </summary>
		public const int BlockFrameCapacity = 32;

		/// <summary>
		///		Flag to determine that the serialized object has an additional identifier.
		///	</summary>
		public const byte FlagEntity = 1 << 1;

		/// <summary>
		///		Flag to determine that the serialized object has additional data.
		/// </summary>
		public const byte FlagBody   = 1 << 3;

		/// <summary>
		///		Flag to determine that the serialized object is called by Begin.
		/// </summary>
		public const byte FlagObject = 1 << 7;


		[NonSerialized] public int EntityIndex;
		[NonSerialized] public int BlockIndex;
		[NonSerialized] public array<telemetry.Block> Blocks;
		[NonSerialized] public array<TypeInfo> Types;
		[NonSerialized] public array<string> StringCache;
		[NonSerialized] public telemetry.Block Current;
		[NonSerialized] public int CurrentBody;
		[NonSerialized] public byte CurrentFlags;
		[NonSerialized] public slice<byte> CurrentBodyPtr;

		public void Initialize()
		{
			var types = Types;
			//types = types.Append(TypeInfo.u8);
			//types = types.Append(TypeInfo.u16);
			//types = types.Append(TypeInfo.u32);
			//types = types.Append(TypeInfo.s32);
			//types = types.Append(TypeInfo.r32);
			//types = types.Append(TypeInfo.r64);
			//types = types.Append(TypeInfo.str);
			//types = types.Append(TypeInfo.double3);
			//types = types.Append(TypeInfo.double4);
			//types = types.Append(TypeInfo.vec3);
			//types = types.Append(TypeInfo.vec4);
			//types = types.Append(TypeInfo.double3x3);
			//types = types.Append(TypeInfo.double4x4);
			Types = types;
		}

		/// <summary>
		///		Return the object to its original state after constructing,
		///		but keeping the allocated memory.
		/// </summary>
		public void Clear()
		{
			Blocks.length = 0;
			Types.length = 0;
			StringCache.length = 0;
			Initialize();
		}

		public int EntityId() => ++EntityIndex;

		public bool AddType(TypeInfo info)
		{
			for (var i = 0; i < Types.length; i++)
			{
				var type = Types[i];
				if (type == info)
				{
					Debug.LogError($"{info} is already in the type list. Ignoring insertion..");
					return false;
				}

				if (type.type == info.type)
				{
					Debug.LogError($"Failed to add {info}, another instance with the same type identifier is already in the type list. Keeping the original {type}; ignoring insertion..");
					return false;
				}
			}

			Types = Types.Append(info);
			return true;
		}

		/// <see cref="Pool.request(int, bool)"/>
		public slice<byte> Request(int length, bool allocate)
		{
			Debug.Assert(Current.frames != null);
			var pool = Current.frames;
			return pool.request(length, allocate);
		}

		/// <see cref="buffer.write{T}(slice{byte}, T)"/>
		[SecurityCritical, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int Write_<T>(T arg) where T : unmanaged
		{
			var data = Request(sizeof(T), true);
			return buffer.write(data, arg);
		}

		/// <summary>
		///		Writing a string in the format of:
		///		string length [4 bytes integer].
		///		string data [string length bytes].
		///	</summary>
		/// <seealso cref="buffer.write{T}(slice{byte}, T)"/>
		[SecurityCritical, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int Write_(string arg)
		{
			var length = 0;
			var capacity = arg.Length + 16;
			var buffer = Request(capacity + 4, false);
			fixed (byte* result = buffer)
			fixed (char* src = arg)
			{
				length = Encoding.UTF8.GetBytes(src, arg.Length, result + 4, capacity);
				insitu.buffer.write(result, length);
				Request(length + 4, true);
			}
			return length;
		}

		/// <summary>Writing data with body length checks.</summary>
		/// <seealso cref="Write_{T}(T)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Body_<T>(T arg) where T : unmanaged
		{
			Debug.Assert((CurrentFlags & FlagBody) != 0, error.AssertNoBody);
			CurrentBody += Write_(arg);
		}

		/// <summary>Writing a string with body length checks.</summary>
		/// <seealso cref="Write_(string)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Body_(string arg)
		{
			Debug.Assert((CurrentFlags & FlagBody) != 0, error.AssertNoBody);
			CurrentBody += Write_(arg);
		}

		/// <summary>
		///		Mark the beginning of serializing an object.
		/// </summary>
		/// <param name="type">Type identifier</param>
		/// <param name="flags">
		///		Object features.
		///		<see cref="FlagEntity"/>
		///		<see cref="FlagBody"/>
		/// </param>
		/// <param name="version"></param>
		/// <param name="id"></param>
		public void Begin(ushort type, byte flags, byte version, int id = 0)
		{
			Debug.Assert(CurrentFlags == 0);
			Debug.Assert(CurrentBody == 0);
			flags |= FlagObject;
			Write_(type);
			Write_(flags);
			Write_(version);

			if ((flags & FlagEntity) != 0)
				Write_(id);

			CurrentFlags = flags;
			if ((flags & FlagBody) != 0)
				CurrentBodyPtr = Request(4, true);
		}

		/// <summary>
		///		Complete the object and if the body flag has been set, the length of the entity is written.
		/// </summary>
		public void End()
		{
			if ((CurrentFlags & FlagBody) != 0)
				buffer.write(CurrentBodyPtr, CurrentBody);

			CurrentFlags = 0;
			CurrentBody = 0;
		}

		public void Write(string value) => Body_(value);

		public void Write(byte value) => Body_(value);

		public void Write(ushort value) => Body_(value);

		public void Write(int value) => Body_(value);

		public void Write(uint value) => Body_(value);

		public void Write(float value) => Body_(value);

		public void Write(double value) => Body_(value);

		public void Write(Color value)
		{
			Body_(value.r);
			Body_(value.g);
			Body_(value.b);
			Body_(value.a);
		}

		public void Write(Vector3 value)
		{
			Body_(value.x);
			Body_(value.y);
			Body_(value.z);
		}

		public void Write(Quaternion value)
		{
			Body_(value.x);
			Body_(value.y);
			Body_(value.z);
			Body_(value.w);
		}

		public void Write(double3 value)
		{
			Body_(value.x);
			Body_(value.y);
			Body_(value.z);
		}

		public void Write(double4 value)
		{
			Body_(value.x);
			Body_(value.y);
			Body_(value.z);
			Body_(value.w);
		}

		public void Write(double4x4 value)
		{
			Body_(value.m00);
			Body_(value.m01);
			Body_(value.m02);
			Body_(value.m03);

			Body_(value.m10);
			Body_(value.m11);
			Body_(value.m12);
			Body_(value.m13);

			Body_(value.m20);
			Body_(value.m21);
			Body_(value.m22);
			Body_(value.m23);

			Body_(value.m30);
			Body_(value.m31);
			Body_(value.m32);
			Body_(value.m33);
		}

		/// <summary>
		///		Used in combination with <see cref="WriteCached(string)"/> and/or <see cref="CacheWithoutWrite(string)"/>.
		///		You can store the cached index to skip table lookups and use this procedure.
		/// </summary>
		public int WriteCached(int index)
		{
			Body_(index);
			return index;
		}

		/// <summary>
		///		Looks if the <paramref name="value"/> has been cached, if it is not is appends it to the cache list.
		///		Subsequently it writes the cached index instead of the string value.
		/// </summary>
		/// <returns>
		///		Cached index.
		///		<seealso cref="WriteCached(int)"/>
		///	</returns>
		public int WriteCached(string value)
		{
			var index = 0;
			var cache = StringCache;
			for (; index < cache.length; index++)
			{
				var entry = cache[index];
				if (string.Equals(entry, value))
					return WriteCached(index);
			}

			StringCache = cache.Append(value);
			return WriteCached(index);

        }

		/// <summary>
		///		Looks if the <paramref name="value"/> has been cached, if it is not is appends it to the cache list.
		/// </summary>
		/// <returns>
		///		Cached index.
		///		<seealso cref="WriteCached(int)"/>
		///	</returns>
		public int CacheWithoutWrite(string value)
		{
			var index = 0;
			var cache = StringCache;
			for (; index < cache.length; index++)
			{
				var entry = cache[index];
				if (string.Equals(entry, value))
					return index;
			}

			StringCache = cache.Append(value);
			return index;
		}

		/// <summary>
		///		Write a new block
		/// </summary>
		public array<telemetry.Block> NewBlock()
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
			BlockIndex++;
			return blocks;
		}

		/// <summary>
		///		Begin a new frame.
		/// </summary>
		public void NewFrame(int frame_id, float frame_time)
		{
			Debug.Log("New Frame: " + frame_id);

			if (Blocks.length == 0)
			{
				NewBlock();
			}

			var blocks = Blocks;
			var block = blocks.last;
			if (block.frame_count >= BlockFrameCapacity)
			{
				NewBlock();
				blocks = Blocks;
				block = blocks.last;
			}

			block.frame_count++;
			blocks.last = block;
			Current = block;
			Frame.Write(this, frame_id, frame_time);
		}

		/// <summary>
		///		This is a wrapper method of: <see cref="Save(Stream, array{telemetry.Block}, Json.Object, array{TypeInfo}, array{string})"/>.
		///		Creates a FileStream of <paramref name="path"/>,
		///		and writes the telemetry data to the stream.
		/// </summary>
		/// <param name="path">
		///		Path where the compressed telemetry data is saved.
		///		Recommended is to have a .bytes extension to allow for serialized reference within the Unity editor.
		///	</param>
		/// <param name="info">
		///		Meta data object data.
		///		Note that additional data gets written to the object, so make sure the instance is mutable.
		/// </param>
		public void Save(string path, Json.Object info)
		{
			var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			Save(stream, Blocks, info, Types, StringCache);
			stream.Flush();
			stream.Close();
		}

		/// <summary>
		///		This is a wrapper method of: <see cref="Save(Stream, array{telemetry.Block}, Json.Object, array{TypeInfo}, array{string})"/>.
		///		Creates a FileStream of <paramref name="path"/> like <see cref="Save(string, Json.Object)"/>,
		///		but first puts it through a GZipStream.
		/// </summary>
		/// <param name="path">
		///		Path where the compressed telemetry data is saved.
		///		Recommended is to have a .gz extension; as it is as GZip file.
		///	</param>
		/// <param name="info">
		///		Meta data object data.
		///		Note that additional data gets written to the object, so make sure the instance is mutable.
		/// </param>
		public void SaveCompressed(string path, Json.Object info)
		{
			var file_stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			var stream = new GZipStream(file_stream, CompressionMode.Compress);
			Save(stream, Blocks, info, Types, StringCache);
			stream.Flush();
			stream.Close();
		}

		/// <summary>
		///		Write telemetry data to <paramref name="stream"/>
		/// </summary>
		/// <returns>
		///		FileWriter containing <paramref name="stream"/> and a cache large enough to fit the largest found string.
		/// </returns>
		public static FileWriter Save(Stream stream, array<telemetry.Block> blocks, Json.Object info, array<TypeInfo> types, array<string> string_cache)
		{
			if (info == null) info = new Json.Object();
			info["operatingSystem"] = SystemInfo.operatingSystem;
			info["processorType"] = SystemInfo.processorType;
			info["processorFrequency"] = SystemInfo.processorFrequency;
			info["processorCount"] = SystemInfo.processorCount;
			info["systemMemorySize"] = SystemInfo.systemMemorySize;
			info["deviceUniqueIdentifier"] = SystemInfo.deviceUniqueIdentifier;
			info["deviceName"] = SystemInfo.deviceName;
			info["deviceModel"] = SystemInfo.deviceModel;
			info["graphicsDeviceName"] = SystemInfo.graphicsDeviceName;
			info["graphicsDeviceVendor"] = SystemInfo.graphicsDeviceVendor;
			info["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion;
			info["version"] = Application.version;
			info["unityVersion"] = Application.unityVersion;
			info["buildGUID"] = Application.buildGUID;
			info["identifier"] = Application.identifier;
			info["productName"] = Application.productName;
			info["companyName"] = Application.companyName;
			info["isEditor"] = Application.isEditor;

				var cache_size = 1024;
				{
					for (var i = 0; i < string_cache.length; i++)
					{
						var element = string_cache[i];
						var length = Encoding.UTF8.GetByteCount(element);
						if (length > cache_size)
							cache_size = length;
					}
					cache_size = (cache_size + Pool.mask) & ~Pool.mask;
				}

				var cache = new byte[cache_size];
				var writer = new FileWriter(stream, cache);
				writer.Begin(telemetry.File.TypeId, 0, telemetry.File.Version);

				JsonData.Write(writer, info);

				for (var i = 0; i < string_cache.length; i++)
					CachedString.Write(writer, string_cache[i]);

				var fields = TypeInfo.Flatten(types, default);


				TypeInfo.Validate(types, fields);


				for (var i = 0; i < fields.length; i++)
					FieldInfo.Write(writer, fields[i]);

				for (var i = 0; i < types.length; i++)
					TypeInfo.Write(writer, types[i]);

				for (var i = 0; i < blocks.length; i++)
					telemetry.Block.Write(writer, blocks[i]);
			
			return writer;

        }
	}
}
