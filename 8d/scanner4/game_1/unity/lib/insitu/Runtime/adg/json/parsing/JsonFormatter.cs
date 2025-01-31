using System.Collections;
using System.Globalization;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Default JSON formatter with no spacing
		/// </summary>
		public static readonly Formatter Minified = new Formatter();

		/// <summary>
		///  Pretty JSON formatter which has spaces, new lines and indentation
		/// </summary>
		/// <remarks>
		///  Indents using tabs
		/// </remarks>
		public static readonly Formatter Pretty = new Formatter
		{
			IndentString = "\t",
			ObjectEmpty = "{ }",
			ObjectOpen = "{\n",
			ObjectDeclare = "\n",
			ArrayEmpty = "[]",
			ArrayOpen = "[\n",
			ArrayDeclare = "\n",
			TokenClose = "\": "
		};


		/// <summary>
		///  Responsible for formatting objects to JSON
		/// </summary>
		public sealed class Formatter
		{
			public string IndentString = "";
			public string ObjectEmpty = "{}";
			public string ObjectOpen = "{";
			public string ObjectClose = "}";
			public string ObjectComma = ",";
			public string ObjectDeclare = "";
			public string ArrayEmpty = "[]";
			public string ArrayOpen = "[";
			public string ArrayClose = "]";
			public string ArrayComma = ",";
			public string ArrayDeclare = "";
			public string TokenOpen = "\"";
			public string TokenClose = "\":";

			/// <summary>
			///  Called when a null value has been read
			///  and appends it to the builder
			/// </summary>
			/// <param name="builder">
			///  Not Null; stores null value to builder
			/// </param>
			/// <returns>
			///  builder
			/// </returns>
			public StringBuilder Null(StringBuilder builder) =>
				builder.Append("null");

			/// <summary>
			///  Called when a boolean value has been read
			///  and appends "true" or "false" to the builder
			/// </summary>
			/// <param name="builder">
			///  Not Null; stores value to builder
			/// </param>
			/// <param name="value">
			///  true or false
			/// </param>
			/// <returns>
			///  builder
			/// </returns>
			public StringBuilder Bool(
				StringBuilder builder,
				bool value) => value
					? builder.Append("true")
					: builder.Append("false");

			/// <summary>
			///  Called when a number/decimal value has been read
			///  and appends its value to the builder
			/// </summary>
			/// <param name="builder">
			///  Not Null; stores value to builder
			/// </param>
			/// <param name="value">
			///  number which is appended to the builder
			/// </param>
			/// <returns>
			///  builder
			/// </returns>
			public StringBuilder Number(
				StringBuilder builder,
				decimal value) =>
				builder.Append(value.ToString(NumberFormatInfo.InvariantInfo));

			/// <summary>
			///  Called when a string value has been read
			///  and appends its value to the builder
			/// </summary>
			/// <param name="builder">
			///  Not Null; stores value to builder
			/// </param>
			/// <param name="value">
			///  string which is escaped and encapsulated in <c>""</c>
			///  and appends it to the builder
			/// </param>
			/// <returns>
			///  builder
			/// </returns>
			public StringBuilder String(
				StringBuilder builder,
				string value)
			{
				if (value == null)
				{
					builder.Append("null");
					return builder;
				}

				// Encapsulate string literal in quotes
				builder.Append('\"');
				value.Escape(builder);
				return builder.Append('\"');
			}

			/// <summary>
			///  Called when an array has been read
			///  and appends its value to the builder
			/// </summary>
			/// <param name="builder">
			///  Not Null; stores value to builder
			/// </param>
			/// <param name="value">
			///  array encapsulated in <c>[]</c>
			///  which is appended to the builder
			/// </param>
			/// <param name="indent">
			///  Object inception level;
			///  how many indents should be used
			/// </param>
			/// <returns>
			///  builder
			/// </returns>
			public StringBuilder Array(
				StringBuilder builder,
				IList value,
				int indent)
			{
				if (value == null)
				{
					builder.Append("null");
					return builder;
				}

				// Return as value, empty array
				if (value.Count < 1)
					return builder.Append(ArrayEmpty);

				// Start the declaration of the array
				// and increase inception level
				builder.Append(ArrayOpen);
				indent++;

				// Serialize all child objects
				for (var i = 0; i < value.Count; i++)
				{
					// Add indent string indent amount of times
					builder.Repeat(IndentString, indent);

					// Serialize value at index with the current formatter
					Stringify(value[i], builder, this, indent);

					// Determines if there are any objects left
					if (i < value.Count - 1)
						builder.Append(ArrayComma);

					// Append suffix to array
					builder.Append(ArrayDeclare);
				}

				// Close the array
				builder.Repeat(IndentString, indent - 1);
				return builder.Append(ArrayClose);
			}

			/// <summary>
			///  Called when a dictionary has been read
			///  and appends its value to the builder
			/// </summary>
			/// <param name="builder">
			///  Not Null; stores value to builder
			/// </param>
			/// <param name="value">
			///  dictionary encapsulated in <c>{}</c>
			///  which is appended to the builder
			/// </param>
			/// <param name="indent">
			///  Object inception level;
			///  how many indents should be used
			/// </param>
			/// <returns>
			///  builder
			/// </returns>
			public StringBuilder Dictionary(
				StringBuilder builder,
				IDictionary value,
				int indent)
			{
				if (value == null)
				{
					builder.Append("null");
					return builder;
				}

				// Retrieve all its keys
				var keys = value.Keys;
				var count = keys.Count;

				// Return as value, empty object
				if (count < 1)
					return builder.Append(ObjectEmpty);

				// Start the declaration of the dictionary as object
				// and increase inception level
				builder.Append(ObjectOpen);
				indent++;

				// Serialize all child objects
				foreach (var key in keys)
				{
					count--;

					// Add indent string indent amount of times
					builder.Repeat(IndentString, indent);

					// Add key value
					builder.Append(TokenOpen);
					key.ToString().Escape(builder);
					builder.Append(TokenClose);

					// Serialize value at index with the current formatter
					var item = value[key];
					Stringify(item, builder, this, indent);

					// Determines if there are any objects left
					if (count > 0)
						builder.Append(ObjectComma);

					// Append suffix to dictionary
					builder.Append(ObjectDeclare);
				}

				// Close the dictionary
				builder.Repeat(IndentString, indent - 1);
				return builder.Append(ObjectClose);
			}

			/// <summary>
			///  Called when an object has been read
			///  and appends its value to the builder
			/// </summary>
			/// <param name="builder">
			///  Not Null; stores value to builder
			/// </param>
			/// <param name="obj">
			///  object encapsulated in <c>{}</c>
			///  which is appended to the builder
			/// </param>
			/// <param name="indent">
			///  Object inception level;
			///  how many indents should be used
			/// </param>
			/// <returns>
			///  builder
			/// </returns>
			/// <seealso cref="IFormatter" />
			public StringBuilder Object(
				StringBuilder builder,
				object obj,
				int indent)
			{
				if (obj == null)
				{
					builder.Append("null");
					return builder;
				}

				// Let object format itself
				if (obj is IFormatter formatter)
					return formatter.Stringify(builder, this, indent);

				var type = obj.GetType();
				UnityEngine.Debug.LogError($"Type {type.Name} is not supported! Add the Introspect attribute to the type.");
				return builder.Append(ObjectEmpty);

				/*// Retrieve cached type info
				var type = obj.GetType();
				var structure = Introspect.StructureOf(type);
				if (structure == null)
				{
					UnityEngine.Debug.LogError($"Type {type.Name} is not supported! Add the Introspect attribute to the type.");
					return builder.Append(ObjectEmpty);
				}

				var length = builder.Length;
				builder.Append(ObjectOpen);

				var count = 0;
				var fields = structure.Fields;
				for (var i = 0; i < fields.Length; i++)
				{
					// Test if the child should be serialized
					var field = fields[i];
					if (field.Get == null ||
						JsonAttribute(field, out var attribute) == false)
						continue;

					var value = field.Get(obj);

					// Determines if there are any objects left
					if (count > 0)
					{
						builder.Append(ObjectComma);
						builder.Append(ObjectDeclare);
					}

					// Declare token
					builder.Repeat(IndentString, indent);
					builder.Append(TokenOpen);
					count++;

					// Get custom name or its member name
					var name = attribute == null || attribute.Name.Empty()
						? field.Name
						: attribute.Name;

					name.Escape(builder);

					// Close token
					builder.Append(TokenClose);

					// Get json value
					Stringify(value, builder, this, indent);
				}

				if (count == 0)
				{
					builder.Length = length;
					return builder.Append(ObjectEmpty);
				}

				// Close the dictionary
				builder.Append(ObjectDeclare);
				builder.Repeat(IndentString, indent - 1);
				return builder.Append(ObjectClose);
				*/
			}
		}
	}
}
