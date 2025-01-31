using System;
using System.Globalization;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Json element representing the number value
		/// </summary>
		/// <remarks>
		///  decimal type is used to store number values
		/// </remarks>
		/// <inheritdoc cref="Json" />
		/// <inheritdoc cref="IConvertible{T}" />
		public sealed class Number
			: Json
			, IConvertible<byte>
			, IConvertible<sbyte>
			, IConvertible<short>
			, IConvertible<ushort>
			, IConvertible<int>
			, IConvertible<uint>
			, IConvertible<long>
			, IConvertible<ulong>
			, IConvertible<float>
			, IConvertible<double>
			, IConvertible<decimal>
			, IConvertible<string>
			, IConvertible<DateTime>
			, IEquatable<Number>
			, IEquatable<byte>
			, IEquatable<sbyte>
			, IEquatable<short>
			, IEquatable<ushort>
			, IEquatable<int>
			, IEquatable<uint>
			, IEquatable<long>
			, IEquatable<ulong>
			, IEquatable<float>
			, IEquatable<double>
			, IEquatable<decimal>
			, IComparable<Number>
			, IComparable<byte>
			, IComparable<sbyte>
			, IComparable<short>
			, IComparable<ushort>
			, IComparable<int>
			, IComparable<uint>
			, IComparable<long>
			, IComparable<ulong>
			, IComparable<float>
			, IComparable<double>
			, IComparable<decimal>
			, IComparable
		{
			public decimal Value;


			/// <inheritdoc />
			public override Json Clone(int level = -1) => CloneNumber(level);

			/// <inheritdoc cref="Json.Clone"/>
			public Number CloneNumber(int level = -1)
			{
				return level == 0
					? this
					: new Number(Value);
			}

			/// <inheritdoc />
			byte IConvertible<byte>.Value => Convert.ToByte(Value);

			/// <inheritdoc />
			sbyte IConvertible<sbyte>.Value => Convert.ToSByte(Value);

			/// <inheritdoc />
			short IConvertible<short>.Value => Convert.ToInt16(Value);

			/// <inheritdoc />
			ushort IConvertible<ushort>.Value => Convert.ToUInt16(Value);

			/// <inheritdoc />
			int IConvertible<int>.Value => Convert.ToInt32(Value);

			/// <inheritdoc />
			uint IConvertible<uint>.Value => Convert.ToUInt32(Value);

			/// <inheritdoc />
			long IConvertible<long>.Value => Convert.ToInt64(Value);

			/// <inheritdoc />
			ulong IConvertible<ulong>.Value => Convert.ToUInt64(Value);

			/// <inheritdoc />
			float IConvertible<float>.Value => Convert.ToSingle(Value);

			/// <inheritdoc />
			double IConvertible<double>.Value => Convert.ToDouble(Value);

			/// <inheritdoc />
			decimal IConvertible<decimal>.Value => Value;

			/// <inheritdoc />
			string IConvertible<string>.Value => Value.ToString(CultureInfo.InvariantCulture);

			/// <inheritdoc />

			DateTime IConvertible<DateTime>.Value => DateTime.FromBinary((long)Value);

			public Number() { }
			public Number(decimal value) => Value = value;
			public Number(sbyte value) => Value = value;
			public Number(byte value) => Value = value;
			public Number(short value) => Value = value;
			public Number(ushort value) => Value = value;
			public Number(int value) => Value = value;
			public Number(uint value) => Value = value;
			public Number(long value) => Value = value;
			public Number(ulong value) => Value = value;
			public Number(float value) => Value = (decimal)value;
			public Number(double value) => Value =(decimal)value;


			/// <inheritdoc />
			public override object As(Type type)
			{
				if (type == typeof(sbyte)) return (sbyte)Value;
				if (type == typeof(byte)) return (byte)Value;
				if (type == typeof(short)) return (short)Value;
				if (type == typeof(ushort)) return (ushort)Value;
				if (type == typeof(int)) return (int)Value;
				if (type == typeof(uint)) return (uint)Value;
				if (type == typeof(long)) return (long)Value;
				if (type == typeof(ulong)) return (ulong)Value;
				if (type == typeof(float)) return (float)Value;
				if (type == typeof(double)) return (double)Value;
				if (type == typeof(decimal)) return Value;
				return base.As(type);
			}

			/// <summary>
			///  Set the value of the object
			/// </summary>
			/// <param name="value">
			///  new value
			/// </param>
			/// <returns>
			///  this
			/// </returns>
			/// <see cref="Value" />
			public Json Set(decimal value)
			{
				Value = value;
				return this;
			}

			/// <inheritdoc />
			public override StringBuilder Stringify(
				StringBuilder builder,
				Formatter formatter,
				int indent = 0) =>
				formatter.Number(builder, Value);

			/// <inheritdoc />
			public override string ToString()
				=> Value.ToString(CultureInfo.InvariantCulture);

			public static bool operator !=(Number json, decimal value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, decimal value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, sbyte value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, sbyte value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, byte value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, byte value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, short value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, short value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, ushort value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, ushort value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, int value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, int value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, uint value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, uint value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, long value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, long value) =>
				json != null && json.Value == value;
			public static bool operator !=(Number json, ulong value) =>
				json == null || json.Value != value;
			public static bool operator ==(Number json, ulong value) =>
				json != null && json.Value == value;

			public static implicit operator Number(byte value) =>
				new Number {Value = value};
			public static implicit operator Number(sbyte value) =>
				new Number {Value = value};
			public static implicit operator Number(short value) =>
				new Number {Value = value};
			public static implicit operator Number(ushort value) =>
				new Number {Value = value};
			public static implicit operator Number(int value) =>
				new Number {Value = value};
			public static implicit operator Number(uint value) =>
				new Number {Value = value};
			public static implicit operator Number(long value) =>
				new Number {Value = value};
			public static implicit operator Number(ulong value) =>
				new Number {Value = value};
			public static implicit operator Number(float value) =>
				new Number {Value = (decimal)value};
			public static implicit operator Number(double value) =>
				new Number {Value = (decimal)value};
			public static implicit operator Number(decimal value) =>
				new Number {Value = value};

			public static bool operator <(Number left, Number right) => left.CompareTo(right) < 0;
			public static bool operator <=(Number left, Number right) => left.CompareTo(right) <= 0;
			public static bool operator >(Number left, Number right) => left.CompareTo(right) > 0;
			public static bool operator >=(Number left, Number right) => left.CompareTo(right) >= 0;

			/// <inheritdoc />
			public bool Equals(Number other) =>
				other != null && Value == other.Value;

			/// <inheritdoc />
			public bool Equals(byte other) => this == other;
			/// <inheritdoc />
			public bool Equals(sbyte other) => this == other;
			/// <inheritdoc />
			public bool Equals(short other) => this == other;
			/// <inheritdoc />
			public bool Equals(ushort other) => this == other;
			/// <inheritdoc />
			public bool Equals(int other) => this == other;
			/// <inheritdoc />
			public bool Equals(uint other) => this == other;
			/// <inheritdoc />
			public bool Equals(long other) => this == other;
			/// <inheritdoc />
			public bool Equals(ulong other) => this == other;
			/// <inheritdoc />
			public bool Equals(float other) => this == other;
			/// <inheritdoc />
			public bool Equals(double other) => this == other;
			/// <inheritdoc />
			public bool Equals(decimal other) => this == other;

			/// <inheritdoc />
			public int CompareTo(Number other) => Value.CompareTo(other.Value);
			/// <inheritdoc />
			public int CompareTo(byte other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(sbyte other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(short other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(ushort other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(int other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(uint other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(long other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(ulong other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(float other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(double other) => Value.CompareTo(other);
			/// <inheritdoc />
			public int CompareTo(decimal other) => Value.CompareTo(other);

			/// <inheritdoc />
			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;

				if (ReferenceEquals(this, obj))
					return true;

				switch (obj) {
					case Number number: return Value == number.Value;
					case byte b: return this == b;
					case sbyte sb: return this == sb;
					case short s: return this == s;
					case ushort us: return this == us;
					case int i: return this == i;
					case uint u: return this == u;
					case long l: return this == l;
					case ulong ul: return this == ul;
					case float f: return this == f;
					case double d: return this == d;
					case decimal dec: return this == dec;
					default: return false;
				}
			}

			/// <inheritdoc />
			public override int GetHashCode() => Value.GetHashCode();

			/// <inheritdoc />
			public int CompareTo(object obj) => Value.CompareTo(obj);
			public override bool Equals(Json other) =>
				other is Number number && number.Value == Value;
		}
	}
}
