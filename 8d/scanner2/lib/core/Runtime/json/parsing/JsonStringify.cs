using System.Collections;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Stringify an object as JSON
		/// </summary>
		/// <param name="obj">
		///  Object to serialize to JSON
		/// </param>
		/// <param name="builder">
		///  Builder used to store the created string.
		///  Consider caching the builder.
		///  Creates a new one when set to null
		///  <remarks>
		///   RESET BUILDER BEFORE USING THIS METHOD!
		///   <c>builder.Length = 0</c>
		///  </remarks>
		/// </param>
		/// <param name="formatter">
		///  Responsible for formatting the object.
		///  When null it uses <c>Json.Minified</c>.
		///  For pretty-print use <c>Json.Pretty</c>
		/// </param>
		/// <param name="indent">
		///  The inception level of parsing an object.
		///  Leave at 0
		/// </param>
		/// <returns>
		///  JSON string in a <c>StringBuilder</c>
		///  Use <c>.ToString()</c> To convert to a string
		/// </returns>
		/// <remarks>
		///  Do not include objects pointing to each other,
		///  this causes infinite loops.
		/// </remarks>
		/// <seealso cref="Minified" />
		/// <seealso cref="Pretty" />
		/// <seealso cref="DefaultStringCapacity" />
		/// <seealso cref="StringBuilder" />
		/// <seealso cref="Formatter" />
		public static StringBuilder Stringify(
			object obj,
			StringBuilder builder = null,
			Formatter formatter = null,
			int indent = 0)
		{
			// Use default formatter when not set
			if (formatter == null)
				formatter = Minified;

			// Create a builder when not set
			if (builder == null)
				builder = new StringBuilder(DefaultStringCapacity);

			switch (obj)
			{
				case null:    return formatter.Null(builder);
				case Json json:  return json.Stringify(builder, formatter, indent);
				case bool o:  return formatter.Bool(builder, o);
				case byte u8: return formatter.Number(builder, u8);
				case short s16:  return formatter.Number(builder, s16);
				case int s32: return formatter.Number(builder, s32);
				case long s64:   return formatter.Number(builder, s64);
				case float r32:  return formatter.Number(builder, (decimal)r32);
				case double r64: return formatter.Number(builder, (decimal)r64);
				case sbyte s8:   return formatter.Number(builder, s8);
				case ushort u16: return formatter.Number(builder, u16);
				case uint u32:   return formatter.Number(builder, u32);
				case ulong u64:  return formatter.Number(builder, u64);
				case decimal d:  return formatter.Number(builder, d);
				case string s:   return formatter.String(builder, s);
				case IList list: return formatter.Array(builder, list, indent);
				case IDictionary dictionary:
					return formatter.Dictionary(builder, dictionary, indent);
			}

			// Use reflection to extract data and go deeper
			return formatter.Object(builder, obj, indent);
		}
	}
}
