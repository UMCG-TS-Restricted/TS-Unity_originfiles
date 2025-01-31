using System.Text;
using UnityEngine;

namespace insitu.telemetry
{
	/// <summary>
	///		Descriptor of a type to convert binary data to another file format.
	/// </summary>
	public class TypeInfo
	{
		/// <summary>
		///		Serialization type identifier.
		///		This is different from type as this data also needs to be serialized.
		/// </summary>
		public const ushort TypeId = 0x0E01;

		/// <summary>
		///		Serialization type identifier.
		/// </summary>
		public ushort type;

		/// <summary>
		///		Serialization version.
		///		Can be used when deserializing a structure on how to do this.
		///		This allows for changes in structures with the possibility of backwards compatibility.
		/// </summary>
		public byte version;

		/// <summary>
		///		Reserved custom data
		/// </summary>
		public byte flags;

		/// <summary>
		///		Name of the type.
		/// </summary>
		public string name;

		/// <summary>
		///		Fields of the type.
		/// </summary>
		public FieldInfo[] fields;

		/// <summary>
		///		Internal serialized fields of the type.
		///		Use with caution, this is used to serialize the object.
		///		<seealso cref="Flatten"/>
		/// </summary>
		public range fields_;


		/// <summary>
		///		Extract all fields and updates the fields_ field of all types.
		/// </summary>
		/// <returns>fields</returns>
		public static array<FieldInfo> Flatten(array<TypeInfo> types, array<FieldInfo> fields)
		{
			fields.length = 0;
			for (var i = 0; i < types.length; i++)
			{
				var type = types[i];
				var type_fields = type.fields;
				var type_field_length = type_fields == null ? 0 : type_fields.Length;

				type.fields_.offset = fields.length;
				type.fields_.length = type_field_length;

				for (var j = 0; j < type_field_length; j++)
				{
					var type_field = type_fields[j];
					fields = fields.Append(type_field);
				}

				types[i] = type;
			}

			return fields;
		}

		public static void Validate(array<TypeInfo> types, array<FieldInfo> fields)
		{
			for (var i = 0; i < fields.length; i++)
			{
				var field = fields[i];
				var type = field.type;
				var index = -1;
				for (var j = 0; j < types.length; j++)
				{
					var element = types[j];
					if (element.type == type)
					{
						index = j;
						break;
					}
				}

				if (index < 0)
				{
					Debug.LogError(error.TypeOfFieldNotFound(field.type, field.name));
				}
			}
		}

		/// <summary>
		///		Serializes <paramref name="type"/> to <paramref name="writer"/>.
		/// </summary>
		public static void Write(FileWriter writer, TypeInfo type)
		{
			var name = Encoding.UTF8.GetBytes(type.name);
			var total_length = name.Length + 12;
			writer.Begin(TypeId, Telemetry.FlagBody, 1, length: total_length);
			writer.Write(type.type);
			writer.Write(type.version);
			writer.Write(type.flags);
			writer.Write(name.Length);
			writer.Stream.Write(name);
			writer.Write(type.fields_.offset);
			writer.Write(type.fields_.length);
		}

		public static bool Read(Object obj, array<FieldInfo> fields, out TypeInfo type)
		{
			type = default;
			if (obj.type != TypeId)
				return false;

			type = new TypeInfo();

			var reader = obj.Read(default);
			reader = reader.read(out type.type);
			reader = reader.read(out type.version);
			reader = reader.read(out type.flags);
			reader = reader.read(out type.name);
			reader = reader.read(out type.fields_.offset);
			reader = reader.read(out type.fields_.length);

			type.fields = new FieldInfo[type.fields_.length];
			for (var i = 0; i < type.fields_.length; i++)
			{
				var field = fields[i + type.fields_.offset];
				type.fields[i] = field;
			}

			return true;
		}

		public static implicit operator ushort(TypeInfo info) => info == null ? (ushort)0 : info.type;

		public static readonly TypeInfo u8 = new TypeInfo
		{
			type = 0x001,
			version = 1,
			name = "u8",
		};

		public static readonly TypeInfo u16 = new TypeInfo
		{
			type = 0x002,
			version = 1,
			name = "u16",
		};

		public static readonly TypeInfo u32 = new TypeInfo
		{
			type = 0x003,
			version = 1,
			name = "u32",
		};

		public static readonly TypeInfo s32 = new TypeInfo
		{
			type = 0x004,
			version = 1,
			name = "s32",
		};

		public static readonly TypeInfo r32 = new TypeInfo
		{
			type = 0x005,
			version = 1,
			name = "r32",
		};

		public static readonly TypeInfo r64 = new TypeInfo
		{
			type = 0x006,
			version = 1,
			name = "r64",
		};

		public static readonly TypeInfo str = new TypeInfo
		{
			type = 0x007,
			version = 1,
			name = "string",
		};

		public static readonly TypeInfo double3 = new TypeInfo
		{
			type = 0x008,
			version = 1,
			name = "double3",
		};

		public static readonly TypeInfo double4 = new TypeInfo
		{
			type = 0x009,
			version = 1,
			name = "double4",
		};

		public static readonly TypeInfo vec3 = new TypeInfo
		{
			type = 0x00A,
			version = 1,
			name = "vec3",
		};

		public static readonly TypeInfo vec4 = new TypeInfo
		{
			type = 0x00B,
			version = 1,
			name = "vec4",
		};

		public static readonly TypeInfo double3x3 = new TypeInfo
		{
			type = 0x00C,
			version = 1,
			name = "double3x3",
		};

		public static readonly TypeInfo double4x4 = new TypeInfo
		{
			type = 0x00D,
			version = 1,
			name = "double3x3",
		};
	}
}