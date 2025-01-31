using System.Globalization;
using System.Text;


namespace ADG
{
	public static class JsonUtilities
	{
		static readonly StringBuilder Builder = new StringBuilder();

		/// <summary>
		///  Stringifies json without passing StringBuilder
		///  or worrying about null reference exceptions
		/// </summary>
		/// <remarks>
		///  Thread safe
		/// </remarks>
		/// <param name="json"></param>
		/// <param name="formatter">
		///  Json.Minified when leaving null
		/// </param>
		/// <returns>
		///  if <paramref name="json" /> is null; "null"
		///  else <paramref name="json" />.Stringify
		/// </returns>
		public static string Stringify(
			this Json json,
			Json.Formatter formatter = null)
		{
			if (json == null)
				return "null";

			// Use default formatter
			if (formatter == null)
				formatter = Json.Minified;

			// Stringify thread safe
			string output;
			lock (Builder)
			{
				Builder.Length = 0;
				json.Stringify(Builder, formatter);
				output = Builder.ToString();
			}

			return output;
		}

		/// <summary>
		///  Adds token to collection
		/// </summary>
		/// <param name="ctx">
		///  Token storage
		/// </param>
		/// <param name="token">
		///  Token value
		/// </param>
		/// <param name="name">
		///  Token name
		/// </param>
		/// <param name="quoted">
		///  Force to use value as string
		/// </param>
		public static void ParseElement(
			this Json.ICollection ctx,
			string token,
			string name,
			bool quoted)
		{
			if (quoted)
			{
				ctx.Add(name, new Json.String {Value = token});
				return;
			}

			var tmp = token.ToLower();

			switch (tmp)
			{
				// Boolean true
				case "true":
					ctx.Add(name, new Json.Bool { Value = true });
					break;

				// Boolean false
				case "false":
					ctx.Add(name, new Json.Bool { Value = false});
					break;

				// Null value
				case "null":
					ctx.Add(name, null);
					break;

				// String or number
				default:

					// Try to parse string to decimal
					decimal number;
					var isNumber = decimal.TryParse(
						token,
						NumberStyles.Float,
						NumberFormatInfo.InvariantInfo, 
						out number);

					// Determine what type to use
					if (isNumber)
						ctx.Add(name, new Json.Number { Value = number });
					else ctx.Add(name, new Json.String { Value = token });
					break;
			}
		}


		/// <summary>
		///		Escapes special characters to safely serialize the input string.
		/// </summary>
		/// <remarks>
		///		TODO: Support \nnn \xhh \uhhhh \Uhhhhhhhh
		/// </remarks>
		public static StringBuilder Escape(this string value, StringBuilder builder = null)
		{
			if (builder == null)
			{
				var capacity = value.Length;
				capacity += capacity / 2;

				builder = new StringBuilder(capacity);
			}

			for (var i = 0; i < value.Length; i++)
			{
				var c = value[i];
				switch (c)
				{
					case '\\':
						builder.Append("\\\\");
						continue;

					case '\"':
						builder.Append("\\\"");
						continue;

					case '\n':
						builder.Append("\\n");
						continue;

					case '\r':
						builder.Append("\\r");
						continue;

					case '\t':
						builder.Append("\\t");
						continue;

					case '\b':
						builder.Append("\\b");
						continue;

					case '\f':
						builder.Append("\\f");
						continue;

					default:
						builder.Append(c);
						continue;
				}
			}

			return builder;
		}

		public static StringBuilder Repeat(
			this StringBuilder builder,
			string value,
			int count)
		{
			if (string.IsNullOrEmpty(value))
				return builder;

			for (var i = 0; i < count; i++)
				builder.Append(value);

			return builder;
		}
	}
}
