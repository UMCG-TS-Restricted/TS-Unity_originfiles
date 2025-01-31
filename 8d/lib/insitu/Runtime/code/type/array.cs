using System.Runtime.CompilerServices;

namespace insitu
{
	public struct array<T>
	{
		public int length;
		public T[] elements;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array(int len, int capacity)
		{
			length = len;
			elements = new T[capacity];
		}

		#if false
		public T this[int index]
		{
			get
			{
				if (elements == null) throw new System.NullReferenceException();
				if (index < 0) throw new System.IndexOutOfRangeException($"{index} is less than 0");
				if (index >= length) throw new System.IndexOutOfRangeException($"{index} is bigger than {length}");
				if (index >= elements.Length) throw new System.IndexOutOfRangeException($"{index} is bigger than {elements.Length} - even more the set length ({length}) is bigger than the capacity.");
				return elements[index];
			}
			set
			{
				if (elements == null) throw new System.NullReferenceException();
				if (index < 0) throw new System.IndexOutOfRangeException($"{index} is less than 0");
				if (index >= length) throw new System.IndexOutOfRangeException($"{index} is bigger than {length}");
				if (index >= elements.Length) throw new System.IndexOutOfRangeException($"{index} is bigger than {elements.Length} - even more the set length ({length}) is bigger than the capacity.");
				elements[index] = value;
			}
		}
		public T this[uint index]
		{
			get
			{
				if (elements == null) throw new System.NullReferenceException();
				if (index >= length) throw new System.IndexOutOfRangeException($"{index} is bigger than {length}");
				if (index >= elements.Length) throw new System.IndexOutOfRangeException($"{index} is bigger than {elements.Length} - even more the set length ({length}) is bigger than the capacity.");
				return elements[index];
			}
			set
			{
				if (elements == null) throw new System.NullReferenceException();
				if (index >= length) throw new System.IndexOutOfRangeException($"{index} is bigger than {length}");
				if (index >= elements.Length) throw new System.IndexOutOfRangeException($"{index} is bigger than {elements.Length} - even more the set length ({length}) is bigger than the capacity.");
				elements[index] = value;
			}
		}
		public T last
		{
			get => this[length - 1];
			set => this[length - 1] = value;
		}
		#else
		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => elements[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => elements[index] = value;
		}
		public T this[uint index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => elements[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => elements[index] = value;
		}
		public T last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => elements[length - 1];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => elements[length - 1] = value;
		}
		#endif
		

		public static array<T> Erase(array<T> array, int index)
		{
			array.elements[index] = array.elements[array.length - 1];
			array.length--;
			return array;
		}

		public static array<T> Grow(array<T> array, int capacity)
		{
			var elements = array.elements;
			if (elements == null || elements.Length < capacity)
			{
				capacity = (capacity + 15) & ~15;
				elements = new T[capacity];
				if (array.elements != null)
				{
					for (var i = 0; i < array.elements.Length; i++)
						elements[i] = array.elements[i];
				}
				array.elements = elements;
			}
			return array;
		}
		public static array<T> Reuse(array<T> array, int length)
		{
			var elements = array.elements;
			if (elements == null || elements.Length < length)
			{
				var capacity = (length + 15) & ~15;
				elements = new T[capacity];
				array.elements = elements;
			}

			array.length = length;
			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static array<T> UnsafeAppend(array<T> array, T item)
		{
			array[array.length] = item;
			array.length++;
			return array;
		}

		public static array<T> Append(array<T> array)
		{
			array = Grow(array, array.length + 1);
			array.length++;
			return array;
		}
		public static array<T> Append(array<T> array, T item)
		{
			array = Grow(array, array.length + 1);
			array[array.length] = item;
			array.length++;
			return array;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Erase(int index) => Erase(this, index);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Grow(int capacity) => Grow(this, capacity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Reuse(int length) => Reuse(this, length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Append() => Append(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Append(T item) => Append(this, item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> UnsafeAppend(T item) => UnsafeAppend(this, item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T At(int index) => index >= 0 && index < length ? elements[index] : default;

		public bool At(int index, out T value)
		{
			if (index >= 0 && index < length)
			{
				value = elements[index];
				return true;
			}

			value = default;
			return false;
		}
	}
}
