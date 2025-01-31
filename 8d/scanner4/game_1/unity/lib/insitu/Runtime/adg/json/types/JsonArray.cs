using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Json element representing the array value
		/// </summary>
		/// <inheritdoc cref="Json" />
		/// <inheritdoc cref="ICollection" />
		/// <inheritdoc cref="IList{T}" />
		/// <inheritdoc cref="IConvertible{T}" />
		public sealed class Array
			: Json
			, ICollection
			, IList<Json>
			, IConvertible<IList>
			, IConvertible<string>
			, IEquatable<Array>
		{
			public readonly List<Json> Elements;

			public Array()
			{
				Elements = new List<Json>();
			}

			/// <inheritdoc />
			string IConvertible<string>.Value => Stringify();

			/// <inheritdoc />
			public override Json Clone(int level = -1) => CloneArray(level);

			/// <inheritdoc cref="Json.Clone"/>
			public Array CloneArray(int level = -1)
			{
				if (level == 0)
					return this;

				level--;
				var array = new Array();
				array.Elements.Capacity = Elements.Capacity;
				for (var i = 0; i < Elements.Count; i++)
				{
					var element = Elements[i]?.Clone(level);
					array.Elements.Add(element);
				}

				return array;
			}


			/// <inheritdoc />
			public override T As<T>()
			{
				return (T)As(typeof(T));
			}

			/// <inheritdoc />
			public override object As(Type type)
			{
				type = type.GetElementType() ?? type;

				if (typeof(Array) == type ||
					typeof(Array).IsSubclassOf(type))
					return this;

				// TODO: Convert
				//var toArray = AsArray.MakeGenericMethod(type);
				//return toArray.Invoke(this, null);
				return null;
			}

			public void AppendTo<T>(ICollection<T> list)
			{
				for (var i = 0; i < Elements.Count; i++)
					list.Add(Elements[i].As<T>());
			}

			public T[] ToArray<T>()
			{
				var array = new T[Elements.Count];
				for (var i = 0; i < array.Length; i++)
					array[i] = Elements[i].As<T>();
				
				return array;
			}

			/// <inheritdoc />
			public void Add(Json item) => Elements.Add(item);

			public void Add(object item)
			{
				if (item is Json json)
					Elements.Add(json);
				else
					Elements.Add(ParseValue(item));
			}

			void ICollection.Add(string name, Json json) => Elements.Add(json);

			public void AddRangeRaw(IEnumerable<Json> items) => Elements.AddRange(items);

			public void AddRange(IList items)
			{
				for (var i = 0; i < items.Count; i++)
					Add(items[i]);
			}

			/// <inheritdoc />
			public void Clear() => Elements.Clear();

			/// <inheritdoc />
			public bool Contains(Json item) => Elements.Contains(item);

			public bool Contains(string item)
			{
				for (var i = 0; i < Elements.Count; i++)
				{
					if (Elements[i] is String str && str.Value == item)
						return true;
				}

				return false;
			}

			public bool Contains(decimal item)
			{
				for (var i = 0; i < Elements.Count; i++)
				{
					if (Elements[i] is Number str && str.Value == item)
						return true;
				}

				return false;
			}

			/// <inheritdoc />
			public void CopyTo(Json[] array, int arrayIndex) => Elements.CopyTo(array, arrayIndex);

			/// <inheritdoc />
			public bool Remove(Json item) => Elements.Remove(item);

			/// <inheritdoc />
			public int Count => Elements.Count;

			/// <inheritdoc />
			public bool IsReadOnly { get; } = false;

			public int IndexOf<T>(Predicate<T> predicate) where T : Json 
			{
				for (var i = 0; i < Elements.Count; i++)
				{
					if (Elements[i] is T item && predicate(item))
						return i;
				}

				return -1;
			}

			public T FirstOrDefault<T>(Predicate<T> predicate) where T : Json
			{
				for (var i = 0; i < Elements.Count; i++)
				{
					if (Elements[i] is T item && predicate(item))
						return item;
				}

				return null;
			}


			/// <inheritdoc />
			public int IndexOf(Json item) => Elements.IndexOf(item);

			/// <inheritdoc />
			public void Insert(int index, Json item) => Elements.Insert(index, item);

			/// <inheritdoc />
			public void RemoveAt(int index) => Elements.RemoveAt(index);

			/// <inheritdoc />
			public Json this[int index]
			{
				get => Elements[index];
				set => Elements[index] = value;
			}

			public Object ObjectAt(int i) => Elements[i] as Object;

			public Array ArrayAt(int i) => Elements[i] as Array;

			public Number NumberAt(int i) => Elements[i] as Number;

			public String StringAt(int i) => Elements[i] as String;

			public Bool BoolAt(int i) => Elements[i] as Bool;

			/// <inheritdoc />
			public IEnumerator<Json> GetEnumerator() => Elements.GetEnumerator();

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();

			/// <inheritdoc />
			public override StringBuilder Stringify(
				StringBuilder builder,
				Formatter formatter,
				int indent = 0)
			{
				return formatter.Array(
					builder,
					Elements,
					indent);
			}

			public override bool Equals(Json other) => Equals(other as Array);
			public bool Equals(Array other)
			{
				if (ReferenceEquals(this, other))
					return true;

				if (other == null)
					return false;

				var lhs = Elements;
				var rhs = other.Elements;
				if (lhs.Count != rhs.Count)
					return false;

				for (var i = 0; i < lhs.Count; i++)
				{
					var lv = lhs[i];
					var rv = rhs[i];
					if (!ReferenceEquals(lv, rv) && !lv.Equals(rv))
						return false;
				}

				return true;
			}

			public static implicit operator Array(List<Json> value)
			{
				var json = new Array();
				json.Elements.AddRange(value);
				return json;
			}

			public static implicit operator Array(Json[] value)
			{
				var json = new Array();
				json.Elements.AddRange(value);
				return json;
			}

			/// <inheritdoc />
			IList IConvertible<IList>.Value => Elements;
		}
	}
}
