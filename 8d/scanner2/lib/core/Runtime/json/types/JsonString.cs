using System;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Json element representing the string value
		/// </summary>
		/// <inheritdoc cref="Json" />
		/// <inheritdoc cref="IConvertible{T}" />
		public sealed class String
			: Json
			, IConvertible<string>
			, IConvertible<char>
			, IConvertible<Guid>
			, IEquatable<String>
		{
			public string Value;

			public String() { }
			public String(string value) => Value = value;
			public String(char value) => Value = new string(value, 1);


			/// <inheritdoc />
			public override Json Clone(int level = -1) => CloneString(level);

			/// <inheritdoc cref="Json.Clone"/>
			public String CloneString(int level = -1)
			{
				return level == 0
					? this
					: new String(Value);
			}


			/// <inheritdoc />
			string IConvertible<string>.Value => Value;

			/// <inheritdoc />
			char IConvertible<char>.Value => string.IsNullOrEmpty(Value) ? '\0' : Value[0];

			/// <inheritdoc />
			Guid IConvertible<Guid>.Value => new Guid(Value);

			/// <inheritdoc />
			public override object As(Type type)
			{
				if (type == typeof(string))
					return Value;

				if (type == typeof(char))
					return ((IConvertible<char>)this).Value;

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
			public Json Set(string value)
			{
				Value = value;
				return this;
			}

			/// <inheritdoc />
			public override StringBuilder Stringify(
				StringBuilder builder,
				Formatter formatter,
				int indent = 0) =>
				formatter.String(builder, Value);

			/// <inheritdoc />
			public override string ToString() => Value;

			/// <inheritdoc />
			public override bool Equals(object obj) =>
				ReferenceEquals(this, obj)
				|| Value == (string)obj;

			public bool Equals(string s) => Value.Equals(s);

			public bool Equals(string s, StringComparison comparison) =>
				Value.Equals(s, comparison);

			/// <inheritdoc />
			public override int GetHashCode() => Value.GetHashCode();
			public override bool Equals(Json other) => Equals(other as String);
			public bool Equals(String other)
			{
				if (other == null)
					return false;

				return Value.Equals(other.Value);
			}

			public static bool operator !=(String json, string value) =>
				json?.Value != value;

			public static bool operator ==(String json, string value) =>
				json?.Value == value;

			public static explicit operator string(String value) => value.Value;

			public static implicit operator String(string value) =>
				new String {Value = value};
		}
	}
}
