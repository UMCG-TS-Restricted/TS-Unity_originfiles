using System.Runtime.CompilerServices;

namespace insitu
{
	/// <summary>
	///		Struct value of List <typeparamref name="T"/>.
	///		This means that you have to override the return value on array mutations.
	/// </summary>
	/// <remarks>
	///		Some safety features are turned off to improve performance.
	/// </remarks>
	public struct array<T>
	{
		public T[] elements;
		public int length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array(int len, int capacity)
		{
			elements = new T[capacity];
			length = len;
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
		public readonly T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => elements[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => elements[index] = value;
		}
		public readonly T this[uint index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => elements[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => elements[index] = value;
		}
		public readonly T last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => elements[length - 1];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => elements[length - 1] = value;
		}
		#endif
		
		/// <summary>Implicitly gets used in fixed statement.</summary>
		public ref T GetPinnableReference() => ref elements[0];
		
		/// <summary>
		///		Place the last element with the element at <paramref name="index"/> and decrement the array length.
		///		This is more performant as only one memory movement has to be made.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static array<T> Erase(array<T> array, int index)
		{
			array.elements[index] = array.elements[array.length - 1];
			array.length--;
			return array;
		}

		/// <summary>
		///		Ensures the array has a capacity of <paramref name="capacity"/> and sets the length to <paramref name="length"/>.
		///		Note that the data can be wiped, but not guaranteed.
		/// </summary>
		public static array<T> Reuse(array<T> array, int length, int capacity)
		{
			var elements = array.elements;
			if (elements == null || elements.Length < capacity)
			{
				capacity = (length + 15) & ~15;
				elements = new T[capacity];
				array.elements = elements;
			}

			array.length = length;
			return array;
		}

		/// <summary>
		///		Ensures the array has a capacity of <paramref name="capacity"/>.
		///		If a resize needs to be performed, the data is being copied.
		/// </summary>
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

		/// <summary>
		///		Append an empty element to the array and grows the array if needed.
		/// </summary>
		public static array<T> Append(array<T> array)
		{
			array = Grow(array, array.length + 1);
			array.length++;
			return array;
		}
		/// <summary>
		///		Append <paramref name="item"/> to the array and grows the array if needed.
		/// </summary>
		public static array<T> Append(array<T> array, T item)
		{
			array = Grow(array, array.length + 1);
			array[array.length] = item;
			array.length++;
			return array;
		}

		/// <summary>
		///		Append <paramref name="item"/> to the array without any checking.
		///		Can be useful if a Grow operation has been performed before.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static array<T> UnsafeAppend(array<T> array, T item)
		{
			array[array.length] = item;
			array.length++;
			return array;
		}

		/// <see cref="Erase(array{T}, int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Erase(int index) => Erase(this, index);

		/// <see cref="Grow(array{T}, int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Grow(int capacity) => Grow(this, capacity);

		/// <see cref="Reuse(array{T}, int, int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Reuse(int length) => Reuse(this, length, length);

		/// <see cref="Reuse(array{T}, int, int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Reuse(int length, int capacity) => Reuse(this, length, capacity);

		/// <see cref="Append(array{T})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Append() => Append(this);

		/// <see cref="Append(array{T}, T)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> Append(T item) => Append(this, item);

		/// <see cref="UnsafeAppend(array{T}, T)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public array<T> UnsafeAppend(T item) => UnsafeAppend(this, item);

		/// <summary>
		///		Returns the element at index.
		///		If index is out of bounds it returns the default value of <typeparamref name="T"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T At(int index) => index >= 0 && index < length ? elements[index] : default;

		/// <summary>
		///		Stores the element at index in <paramref name="value"/>.
		///		If index is out of bounds it stores the default value of <typeparamref name="T"/> and returns false.
		///		If it is in bounds the procedure returns true.
		/// </summary>
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

		/// <summary>
		///		Clears <paramref name="other"/> and copies the elements to <paramref name="other"/>.
		/// </summary>
		public array<T> CopyTo(array<T> other)
		{
			other = Reuse(other, length, length);
			for (var i = 0; i < length; i++)
				other.elements[i] = elements[i];
			return other;
		}
	}
}
