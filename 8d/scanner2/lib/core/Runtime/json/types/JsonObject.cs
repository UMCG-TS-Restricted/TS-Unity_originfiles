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

			public Array ArrayOf(string key, Array fallback = null)
			{
				if (string.IsNullOrEmpty(key))
					return fallback;

				if (Elements.TryGetValue(key, out var json) && json is Array json_array)
					return json_array;

				return fallback;
			}
			public Object ObjectOf(string key, Object fallback = null)
			{
				if (string.IsNullOrEmpty(key))
					return fallback;

				if (Elements.TryGetValue(key, out var json) && json is Object json_object)
					return json_object;

				return fallback;
			}
			public String StringOf(string key, String fallback = null)
			{
				if (string.IsNullOrEmpty(key))
					return fallback;

				if (Elements.TryGetValue(key, out var json) && json is String json_string)
					return json_string;

				return fallback;
			}
			public Number NumberOf(string key, Number fallback = null)
			{
				if (string.IsNullOrEmpty(key))
					return fallback;

				if (Elements.TryGetValue(key, out var json) && json is Number json_number)
					return json_number;

				return fallback;
			}
			public Bool BoolOf(string key, Bool fallback = null)
			{
				if (string.IsNullOrEmpty(key))
					return fallback;

				if (Elements.TryGetValue(key, out var json) && json is Bool json_bool)
					return json_bool;

				return fallback;
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
