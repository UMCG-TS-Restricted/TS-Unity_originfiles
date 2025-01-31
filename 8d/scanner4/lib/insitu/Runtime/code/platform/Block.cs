namespace insitu
{
	namespace memory
	{
		public unsafe class Block
		{
			public int length;
			public byte[] data;

			/// <summary>Previous full block</summary>
			public Block previous;

			/// <summary>Next empty block</summary>
			public Block next;
		}
	}
}
