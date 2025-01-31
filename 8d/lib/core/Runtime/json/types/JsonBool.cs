using System;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Json element representing the boolean value
		/// </summary>
		/// <inheritdoc cref="Json" />
		/// <inheritdoc cref="IConvertible{T}" />
		public sealed class Bool
			: Json
			, IConvertible<bool>
			, IConvertible<int>
			, IConvertible<string>
		{
			public bool Value;


			public Bool() { }
			public Bool(bool value) { Value = value; }

			public Bool Set(bool value)
			{
				Value = value;
				return this;
			}

			/// <inheritdoc />
			public override Json Clone(int level = -1) => CloneBool(level);

			/// <inheritdoc cref="Json.Clone"/>
			public Bool CloneBool(int level = -1)
			{
				return level == 0
					? this
					: new Bool(Value);
			}

			/// <inheritdoc />
			bool IConvertible<bool>.Value => Value;

			/// <inheritdoc />
			int IConvertible<int>.Value => Value ? 1 : 0;

			/// <inheritdoc />
			string IConvertible<string>.Value => Stringify();

			/// <inheritdoc />
			public override object As(Type type)
			{
				if (type == typeof(bool))
					return Value;

				if (type == typeof(int))
					return Value ? 1 : 0;

				return base.As(type);
			}

			/// <inheritdoc />
			public override StringBuilder Stringify(
				StringBuilder builder,
				Formatter formatter,
				int indent = 0) =>
				formatter.Bool(builder, Value);

			/// <inheritdoc />
			public override string ToString()
				=> Value ? "true" : "false";
			public override bool Equals(Json other)
			{
				var o = other as Bool;
				if (o == null)
					return false;

				return o.Value == Value;
			}

			public static implicit operator Bool(bool value)
				=> new Bool {Value = value};
		}
	}
}
