using UnityEngine;


namespace insitu
{
	namespace memory
	{
		public class Pool
		{
			/// <summary>
			///		Amount of bytes of a memory page.
			/// </summary>
			/// <remarks>
			///		This must by a power of two.
			///		This allows you to use the value as a mask for math shortcuts:
			///		aligned value = (value + (page - 1)) & ~(page - 1)
			///	</remarks>
			public const int page = 4096;
			public const int mask = page - 1;

			public Block head;
			public Block tail;

			public static Pool create()
			{
				var current = new Block
				{
					length = 0,
					data = new byte[page],
					previous = null,
					next = null,
				};

				return new Pool
				{
					head = current,
					tail = current,
				};
			}

			public int size()
			{
				var result = 0;
				var current = head;
				while (current != null)
				{
					result += current.length;
					current = current.previous;
				}
				return result;
			}

			public void recycle()
			{
				var current = head;
				while (current != null)
				{
					current.length = 0;
					current = current.previous;
				}
				head = tail;
			}

			public slice request(int length, bool allocate)
			{
				var current = head;
				if (current.length + length <= current.data.Length)
				{
					var offset = current.length;
					if (allocate)
						current.length += length;

					return new slice
					{
						data = current.data,
						offset = offset,
						length = length,
					};
				}

				var next = current.next;
				if (next != null)
				{
					if (length <= next.data.Length)
					{
						Debug.Log("Allocating a new block! Note this block fully reused.");
						next.length = allocate ? length : 0;
						head = next;
						return new slice
						{
							data = current.data,
							offset = 0,
							length = length,
						};
					}

					Debug.Log("Allocating a new block! Note this block is inserted and exceeded the cached page.");
					var capacity = (length + mask) & ~mask;
					var new_block = new Block
					{
						length = allocate ? length : 0,
						data = new byte[capacity],
						previous = current,
						next = next,
					};
					current.next = new_block;
					next.previous = new_block;
					head = new_block;
					return new slice
					{
						data = new_block.data,
						offset = 0,
						length = length,
					};
				}

				// new
				{
					Debug.Log("Allocating a new block!");
					var capacity = (length + mask) & ~mask;
					var new_block = new Block
					{
						length = allocate ? length : 0,
						data = new byte[capacity],
						previous = current,
						next = null,
					};
					current.next = new_block;
					head = new_block;
					return new slice
					{
						data = new_block.data,
						offset = 0,
						length = length,
					};
				}
			}
		}
	}
}
