using System.Runtime.InteropServices;
using System.Text;


namespace insitu
{
	/// <summary>
	///		A fixed sized 64-byte buffer.
	///		Useful to communicate zero-terminated string data in dynamic linked libraries.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct byte64
	{
		public const int size = 64;
		public fixed byte value[size];

		public override string ToString()
		{
			int length = 0;
			for (var i = 0; i < size; i++)
			{
				if (value[i] == 0)
				{
					length = i;
					break;
				}
			}

			string result;
			fixed (byte* ptr = value)
			{
				result = Encoding.UTF8.GetString(ptr, length);
			}

			return result;
		}
	}
}
