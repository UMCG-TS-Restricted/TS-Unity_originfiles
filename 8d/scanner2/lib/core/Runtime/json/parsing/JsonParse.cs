using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <param name="text">
		///  JSON text to parse
		/// </param>
		/// <param name="token">
		///  Should be cleared before passing a value
		/// </param>
		/// <param name="stack">
		///  Should be cleared before passing a value
		/// </param>
		/// <returns>
		///  ICollection Json object
		/// </returns>
		/// TODO: This is not optimal with dynamic buffers
		/// We need to be able to return an error if one occurs,
		/// but not an exception.
		public static Json Parse(
			string text,
			StringBuilder token = null,
			Stack<ICollection> stack = null)
		{
			if (token == null)
				token = new StringBuilder(DefaultStringCapacity);
			else token.Length = 0;

			if (stack == null)
				stack = new Stack<ICollection>();
			else stack.Clear();

			var tokenName = "";
			var quoteMode = false;
			var tokenIsQuoted = false;
			ICollection ctx = null;

			for (var i = 0; i < text.Length; i++)
			{
				var c = text[i];
				switch (c)
				{
					case '{':
						if (quoteMode)
						{
							token.Append(c);
							break;
						}

						var node = new Object();
						stack.Push(node);
						ctx?.Add(tokenName, node);

						ctx = node;
						tokenName = "";
						token.Length = 0;
						break;

					case '[':
						if (quoteMode)
						{
							token.Append(c);
							break;
						}

						var array = new Array();
						stack.Push(array);
						ctx?.Add(tokenName, array);

						tokenName = "";
						token.Length = 0;
						ctx = array;
						break;

					case '}':
					case ']':
						if (quoteMode)
						{
							token.Append(c);
							break;
						}

						if (stack.Count == 0)
						{
							UnityEngine.Debug.LogError("JSON Parse: Too many closing brackets.\n" + text);
							return ctx as Json;
						}

						stack.Pop();

						if (token.Length > 0 || tokenIsQuoted)
						{
							ctx.ParseElement(
								token.ToString(),
								tokenName,
								tokenIsQuoted);

							tokenIsQuoted = false;
						}

						tokenName = "";
						token.Length = 0;
						if (stack.Count > 0)
							ctx = stack.Peek();

						break;

					case ':':
						if (quoteMode)
						{
							token.Append(c);
							break;
						}

						tokenName = token.ToString();
						token.Length = 0;
						tokenIsQuoted = false;
						break;

					case '"':
						quoteMode ^= true;
						tokenIsQuoted |= quoteMode;
						break;

					case ',':
						if (quoteMode)
						{
							token.Append(c);
							break;
						}

						if (token.Length > 0 || tokenIsQuoted)
						{
							ctx.ParseElement(
								token.ToString(),
								tokenName,
								tokenIsQuoted);
						}

						tokenName = "";
						token.Length = 0;
						tokenIsQuoted = false;
						break;

					case '\r':
					case '\n':
						break;

					case ' ':
					case '\t':
						if (quoteMode)
							token.Append(c);
						break;

					case '\\':
						c = text[++i];
						if (quoteMode)
						{
							switch (c)
							{
								case 't':
									token.Append('\t');
									break;

								case 'r':
									token.Append('\r');
									break;

								case 'n':
									token.Append('\n');
									break;

								case 'b':
									token.Append('\b');
									break;

								case 'f':
									token.Append('\f');
									break;

								case 'u':
									{
										var s = text.Substring(i + 1, 4);
										token.Append((char)int.Parse(
											s,
											NumberStyles.AllowHexSpecifier));

										i += 4;
										break;
									}

								default:
									token.Append(c);
									break;
							}
						}

						break;

					case '\0':
						return ctx as Json;

					default:
						token.Append(c);
						break;
				}
			}

			if (quoteMode)
				UnityEngine.Debug.LogError("JSON Parse: Quotation marks seems to be messed up.\n" + text);

			return ctx as Json;
		}

		/// <see cref="Parse"/>
		public static Object ParseObject(
			string text,
			StringBuilder token = null,
			Stack<ICollection> stack = null) =>
			Parse(text, token, stack) as Object;

		/// <see cref="Parse"/>
		public static Array ParseArray(
			string text,
			StringBuilder token = null,
			Stack<ICollection> stack = null) =>
			Parse(text, token, stack) as Array;

		/// <summary>
		/// Parse object to JSON object
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static Json ParseValue(object obj)
		{
			switch (obj)
			{
				case null:			return null;
				case Json json:		return json;
				case bool b:		return new Bool(b);
				case string s:		return new String(s);
				case char c:		return new String(c);
				case byte u8:		return new Number(u8);
				case short s16:		return new Number(s16);
				case int s32:		return new Number(s32);
				case long s64:		return new Number(s64);
				case float r32:		return new Number(r32);
				case double r64:	return new Number(r64);
				case sbyte s8:		return new Number(s8);
				case ushort u16:	return new Number(u16);
				case uint u32:		return new Number(u32);
				case ulong u64:		return new Number(u64);
				case decimal dec:	return new Number(dec);
				case Guid guid: 	return new String(guid.ToString());
				case IList listValue:
				{
					var list = new Array();
					var count = listValue.Count;
					for (var i = 0; i < count; i++)
					{
						var item = ParseValue(listValue[i]);
						list.Add(item);
					}

					return list;
				}
				case IDictionary dictionaryValue:
				{
					var dictionary = new Object();

					foreach (var key in dictionaryValue.Keys)
					{
						var value = dictionaryValue[key];
						var item = ParseValue(value);

						dictionary.Add(key.ToString(), item);
					}

					return dictionary;
				}
			}

			var type = obj.GetType();
			UnityEngine.Debug.LogError($"Type {type.Name} is not supported! Add the Introspect attribute to the type.");
			return new Object();

			/*
			var container = Introspect.StructureOf(type);
			if (container == null)
			{
				UnityEngine.Debug.LogError($"Type {type.Name} is not supported! Add the Introspect attribute to the type.");
				return new Object();
			}

			var fields = container.Fields;
			var collection = new Object();
			for (var i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				if (field.Get == null ||
					JsonAttribute(field, out var attribute) == false)
					continue;

				// Register json item
				var value = field.Get(obj);
				var item = ParseValue(value);
				var name = attribute == null || attribute.Name.Empty()
					? field.Name
					: attribute.Name;

				collection.Add(name, item);
			}

			return collection;*/
		}

		public static Array ParseArrayValue(IList value)
		{
			var list = new Array();
			var count = value.Count;
			for (var i = 0; i < count; i++)
			{
				var item = ParseValue(value[i]);
				list.Add(item);
			}

			return list;
		}

		public static Object ParseDictionaryValue(IDictionary value)
		{
			var dictionary = new Object();

			foreach (var key in value.Keys)
			{
				var val = value[key];
				var item = ParseValue(val);
				dictionary.Add(key.ToString(), item);
			}

			return dictionary;
		}
	}
}
