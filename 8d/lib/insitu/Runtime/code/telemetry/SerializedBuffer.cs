using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


namespace insitu
{
	public struct SerializedBuffer
	{
		public slice slice;
		public ushort version;
		public bool swap_endian;
	}
}
