using System;
using System.Text;


namespace ADG
{
	/// <summary>
	///  Parse and stringify objects to JSON
	/// </summary>
	/// <remarks>
	///  Stringifying only serializes properties one level deep
	///  unless marked with JsonAttribute
	/// </remarks>
	/// <inheritdoc cref="IFormatter" />
	/// <see href="https://github.com/Bunny83/SimpleJSON" />
	public abstract partial class Json : Json.IFormatter, IEquatable<Json>
	{
		static readonly StringBuilder Builder = new StringBuilder();

		/// <summary>
		///  Default capacity when needing a <c>StringBuilder</c>
		/// </summary>
		public const int DefaultStringCapacity = 512;

		Json() { }

		/// <summary>
		///  Easy-to-use version of Stringify
		/// </summary>
		/// <remarks>
		///  Thread safe, but contains lock guard
		/// </remarks>
		/// <param name="formatter">
		///  null for pretty
		/// </param>
		/// <returns></returns>
		public string Stringify(Formatter formatter = null)
		{
			if (formatter == null)
				formatter = Minified;

			string result;
			lock (Builder)
			{
				Builder.Length = 0;
				Stringify(Builder, formatter);
				result = Builder.ToString();
			}

			return result;
		}

		public byte[] AsUTF8(Formatter formatter = null)
		{
			if (formatter == null)
				formatter = Minified;

			string result;
			lock (Builder)
			{
				Builder.Length = 0;
				Stringify(Builder, formatter);
				result = Builder.ToString();
			}

			return Encoding.UTF8.GetBytes(result);
		}

		/// <inheritdoc />
		public abstract StringBuilder Stringify(
			StringBuilder builder,
			Formatter formatter,
			int indent = 0);

		/// <inheritdoc />
		public override string ToString() => Stringify();
		public byte[] ToBytes() => Encoding.UTF8.GetBytes(ToString());


		/// <summary>
		///	 Copies the the object <paramref name="level"/> serialization levels deep.
		/// </summary>
		/// <param name="level">
		///  How many levels deep to serialize.
		///  -1 will perform a deep clone.
		///  0 will do nothing.
		/// </param>
		/// <returns></returns>
		public abstract Json Clone(int level = -1);

		/// <summary>
		///  Convert or get value as type
		///  TODO: Convert generated data
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>
		///  Default value of T when not convertible to the specified type
		/// </returns>
		public virtual T As<T>() =>
			this is IConvertible<T> converter
				? converter.Value
				: default;

		public virtual object As(Type type)
		{
			if (type == typeof(Json) ||
				type == GetType())
				return this;

			if (type.IsSubclassOf(typeof(Json)))
				return null;

			// TODO: Convert
			throw new NotImplementedException();

			//if (AsInfoT.TryGetValue(type, out var info))
			//	return info.Invoke(this, null);

			//var method = AsInfo.MakeGenericMethod(type);
			//AsInfoT[type] = method;

			//return method.Invoke(this, null);
		}

		public abstract bool Equals(Json other);

		public static implicit operator bool(Json value)
		{
			switch (value)
			{
				case Bool b: return b.Value;
				case Number n: return n.Value != 0m;
				default: return value != null;
			}
		}
		public static implicit operator byte(Json value) => value?.As<byte>() ?? default;
		public static implicit operator sbyte(Json value) => value?.As<sbyte>() ?? default;
		public static implicit operator short(Json value) => value?.As<short>() ?? default;
		public static implicit operator ushort(Json value) => value?.As<ushort>() ?? default;
		public static implicit operator int(Json value) => value?.As<int>() ?? default;
		public static implicit operator uint(Json value) => value?.As<uint>() ?? default;
		public static implicit operator long(Json value) => value?.As<long>() ?? default;
		public static implicit operator ulong(Json value) => value?.As<ulong>() ?? default;
		public static implicit operator float(Json value) => value?.As<float>() ?? default;
		public static implicit operator double(Json value) => value?.As<double>() ?? default;
		public static implicit operator decimal(Json value) => value?.As<decimal>() ?? default;
		public static implicit operator string(Json value) => value?.As<string>();
		public static implicit operator char(Json value)
		{
			var s = value?.As<string>();
			return string.IsNullOrEmpty(s) ? '\0' : s[0];
		}
		public static implicit operator Guid(Json value) => value?.As<Guid>() ?? default;
		public static implicit operator DateTime(Json value) => value?.As<DateTime>() ?? default;

		public static implicit operator Json(bool value) => new Bool(value);
		public static implicit operator Json(byte value) => new Number(value);
		public static implicit operator Json(sbyte value) => new Number(value);
		public static implicit operator Json(short value) => new Number(value);
		public static implicit operator Json(ushort value) => new Number(value);
		public static implicit operator Json(int value) => new Number(value);
		public static implicit operator Json(uint value) => new Number(value);
		public static implicit operator Json(long value) => new Number(value);
		public static implicit operator Json(ulong value) => new Number(value);
		public static implicit operator Json(float value) => new Number(value);
		public static implicit operator Json(double value) => new Number(value);
		public static implicit operator Json(decimal value) => new Number(value);
		public static implicit operator Json(string value) => new String(value);
		public static implicit operator Json(char value) => new String(value);
		public static implicit operator Json(Guid value) => new String(value.ToString());
		public static implicit operator Json(DateTime value) => new Number(value.ToBinary());
	}
}
