using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Json element representing the object and dictionary value
		/// </summary>
		/// <inheritdoc cref="Json" />
		/// <inheritdoc cref="ICollection" />
		/// <inheritdoc cref="IDictionary{TKey,TValue}" />
		public sealed class Object
			: Json
			, IConvertible<string>
			, ICollection
			, IDictionary
			, IDictionary<string, Json>
			, IEquatable<Object>
		{
			public readonly Dictionary<string, Json> Elements;

			/// <inheritdoc />
			public Object()
			{
				Elements = new Dictionary<string, Json>();
			}

			/// <inheritdoc />
			public Json this[string key]
			{
				get
				{
					if (string.IsNullOrEmpty(key))
						return null;

					return Elements.TryGetValue(key, out var json)
						? json
						: null;
				}
				set => Elements[key] = value;
			}

			/// <inheritdoc />
			string IConvertible<string>.Value => Stringify();

			/// <inheritdoc />
			public override Json Clone(int level = -1) => CloneObject(level);

			/// <inheritdoc cref="Json.Clone"/>
			public Object CloneObject(int level = -1)
			{
				if (level == 0)
					return this;

				level--;
				var obj = new Object();
				foreach (var element in Elements)
				{
					var copy = element.Value?.Clone(level);
					obj.Add(element.Key, copy);
				}

				return obj;
			}

			public bool Has(string key) => Elements.ContainsKey(key);

			/// <summary>
			///  Throws KeyNotFoundException when not found.
			/// </summary>
			/// <seealso cref="Of{T}" />
			/// <returns>
			///  _elements[key]
			/// </returns>
			public T RawOf<T>(string key) where T : Json
			{
				return Elements[key] as T;
			}

			/// <summary>
			///  Throws KeyNotFoundException when not found.
			/// </summary>
			/// <seealso cref="Of{T}" />
			/// <returns>
			///  _elements[key]
			/// </returns>
			public Json RawOf(string key)
			{
				return Elements[key];
			}

			/// <seealso cref="RawOf{T}" />
			/// <returns>
			///  Null when not found
			/// </returns>
			public T Of<T>(string key) where T : Json
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as T;

				return null;
			}

			public Array EnsuredArrayOf(string key)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Array;

				var result = new Array();
				Elements[key] = result;
				return result;
			}
			public Object EnsuredObjectOf(string key)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Object;

				var result = new Object();
				Elements[key] = result;
				return result;
			}
			public String EnsuredStringOf(string key, string value)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as String;

				var result = new String(value);
				Elements[key] = result;
				return result;
			}
			public Number EnsuredNumberOf(string key, decimal value = 0)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Number;

				var result = new Number(value);
				Elements[key] = result;
				return result;
			}
			public Bool EnsuredBoolOf(string key, bool value = false)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Bool;

				var result = new Bool(value);
				Elements[key] = result;
				return result;
			}

			public Array ArrayOf(string key)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Array;

				return null;
			}
			public Object ObjectOf(string key)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Object;

				return null;
			}
			public String StringOf(string key)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as String;

				return null;
			}
			public Number NumberOf(string key)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Number;

				return null;
			}
			public Bool BoolOf(string key)
			{
				if (string.IsNullOrEmpty(key))
					return null;

				Json json;
				if (Elements.TryGetValue(key, out json))
					return json as Bool;

				return null;
			}

			public bool Of<T>(string key, out T result) where T : Json
			{
				if (string.IsNullOrEmpty(key))
				{
					result = null;
					return false;
				}

				Json json;
				if (Elements.TryGetValue(key, out json))
				{
					result = json as T;
					return result != null;
				}

				result = null;
				return false;
			}
			public string Of(string key, out string value, string @default = null)
			{
				if (string.IsNullOrEmpty(key))
					return value = @default;

				Json json;
				if (Elements.TryGetValue(key, out json))
				{
					var str = json as String;
					if (str != null)
						return value = str.Value;
				}

				return value = @default;
			}
			public bool Of(string key, out bool value, bool @default = false)
			{
				if (string.IsNullOrEmpty(key))
					return value = @default;

				Json json;
				if (Elements.TryGetValue(key, out json))
				{
					var val = json as Bool;
					if (val != null)
						return value = val.Value;
				}

				return value = @default;
			}



			[Obsolete("Use \"Of\" instead")]
			public T Get<T>(string key) where T : Json
			{
				return Elements[key] as T;
			}

			public void Set(string key, object value)
			{
				Elements[key] = value is Json
					? (Json)value
					: ParseValue(value);
			}

			public void As<T>(ref T output)
			{
				if (typeof(Object) == typeof(T) ||
					typeof(Object).IsSubclassOf(typeof(T)))
				{
					output = (T)(object)this;
					return;
				}

				UnityEngine.Debug.LogError($"Type {typeof(T).Name} is not supported! Add the Introspect attribute to the type.");
				return;
				/*var type = Introspect.StructureOf(typeof(T));
				if (type == null)
				{
					UnityEngine.Debug.LogError($"Type {typeof(T).Name} is not supported! Add the Introspect attribute to the type.");
					return;
				}

				var fields = type.Fields;
				for (var i = 0; i < fields.Length; i++)
				{
					var field = fields[i];
					if (field == null || field.Set == null)
						continue;

					// Try and get if it parsable
					if (JsonAttribute(field, out var attribute) == false)
						continue;

					// Get name
					var name = attribute == null || attribute.Name.Empty()
						? field.Name
						: attribute.Name;

					// Try get name
					if (TryGetValue(name, out var json))
					{
						if (json == null)
						{
							output = (T)field.Set(output, null);
						}
						else
						{
							var value = json.As(field.Type);
							output = (T)field.Set(output, value);
						}
					}
				}*/
			}

			/// <inheritdoc />
			public override T As<T>()
			{
				var output = Activator.CreateInstance<T>();
				As(ref output);
				return output;
			}

			public Dictionary<string, T> AsDictionary<T>()
			{
				var dictionary = new Dictionary<string, T>();
				foreach (var element in Elements)
					dictionary[element.Key] = element.Value.As<T>();

				return dictionary;
			}

			/// <summary>
			///	 Value as T with key of <paramref name="key"/>.
			/// </summary>
			/// <typeparam name="T">
			///  (T)this["key"]
			/// </typeparam>
			/// <param name="key">
			///  this["key"]
			/// </param>
			/// <returns>
			///  When key is not found it will return default(T)
			/// </returns>
			public T OfAs<T>(string key)
			{
				if (string.IsNullOrEmpty(key))
					return default;

				return Elements.TryGetValue(key, out var json)
					? json.As<T>()
					: default;
			}

			/// <inheritdoc />
			public override StringBuilder Stringify(
				StringBuilder builder,
				Formatter formatter,
				int indent = 0)
			{
				return formatter.Dictionary(
					builder,
					Elements,
					indent);
			}

			/// <inheritdoc />
			public ICollection<string> Keys => Elements.Keys;

			/// <inheritdoc />
			System.Collections.ICollection IDictionary.Values => ((IDictionary)Elements).Values;

			/// <inheritdoc />
			System.Collections.ICollection IDictionary.Keys => ((IDictionary)Elements).Keys;

			/// <inheritdoc />
			public ICollection<Json> Values => Elements.Values;

			/// <inheritdoc />
			public void Add(KeyValuePair<string, Json> item) => ((IDictionary<string, Json>)Elements).Add(item);

			/// <inheritdoc />
			void IDictionary.Add(object key, object value) => ((IDictionary)Elements).Add(key, value);

			/// <inheritdoc cref="IDictionary{TKey,TValue}" />
			public void Clear() => Elements.Clear();

			/// <inheritdoc />
			bool IDictionary.Contains(object key) => ((IDictionary)Elements).Contains(key);

			/// <inheritdoc />
			IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)Elements).GetEnumerator();

			/// <inheritdoc />
			void IDictionary.Remove(object key) => ((IDictionary)Elements).Remove(key);

			/// <inheritdoc />
			bool IDictionary.IsFixedSize => ((IDictionary)Elements).IsFixedSize;

			/// <inheritdoc />
			bool IDictionary.IsReadOnly => ((IDictionary)Elements).IsReadOnly;

			/// <inheritdoc />
			object IDictionary.this[object key]
			{
				get => ((IDictionary)Elements)[key];
				set => ((IDictionary)Elements)[key] = value;
			}

			/// <inheritdoc />
			public bool Contains(KeyValuePair<string, Json> item) => ((IDictionary<string, Json>)Elements).Contains(item);

			/// <inheritdoc />
			public void CopyTo(
				KeyValuePair<string, Json>[] array,
				int arrayIndex) =>
				((IDictionary<string, Json>)Elements).CopyTo(array, arrayIndex);

			public void CopyTo(Object other)
			{
				foreach (var item in Elements)
					other[item.Key] = item.Value;
			}

			/// <inheritdoc />
			public bool Remove(KeyValuePair<string, Json> item) => ((IDictionary<string, Json>)Elements).Remove(item);

			/// <inheritdoc />
			void System.Collections.ICollection.CopyTo(System.Array array, int index) => ((System.Collections.ICollection)Elements).CopyTo(array, index);

			/// <inheritdoc cref="IDictionary{TKey,TValue}" />
			public int Count => Elements.Count;

			/// <inheritdoc />
			bool System.Collections.ICollection.IsSynchronized =>
				((System.Collections.ICollection)Elements)
				.IsSynchronized;

			/// <inheritdoc />
			object System.Collections.ICollection.SyncRoot => ((System.Collections.ICollection)Elements).SyncRoot;

			/// <inheritdoc cref="Json.ICollection.Add" />
			void ICollection.Add(string key, Json value) => Elements[key] = value;

			public void Add(string key, Json value) => Elements.Add(key, value);

			/// <inheritdoc />
			public bool ContainsKey(string key) => Elements.ContainsKey(key);

			/// <inheritdoc />
			public bool Remove(string key) => Elements.Remove(key);

			/// <inheritdoc />
			public bool TryGetValue(string key, out Json value) => Elements.TryGetValue(key, out value);

			public IEnumerator<KeyValuePair<string, Json>> GetEnumerator() => Elements.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();

			public bool Equals(Object other)
			{
				if (ReferenceEquals(this, other))
					return true;

				if (other == null)
					return false;

				var lhs = Elements;
				var rhs = other.Elements;
				if (lhs.Count != rhs.Count)
					return false;

				foreach (var le in lhs)
				{
					Json rv;
					if (!rhs.TryGetValue(le.Key, out rv))
						return false;

					if (!ReferenceEquals(le.Value, rv) && !le.Value.Equals(rv))
						return false;
				}

				return true;
			}

			public override bool Equals(Json other) => Equals(other as Object);

			/// <inheritdoc />
			bool ICollection<KeyValuePair<string, Json>>.IsReadOnly => false;
		}
	}
}
