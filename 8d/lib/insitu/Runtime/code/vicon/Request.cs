using System;
using System.Runtime.InteropServices;
using System.Text;

namespace insitu
{
	public static partial class Vicon
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Request<T>
			where T : unmanaged
		{
			public int result;
			public T value;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Request<T0, T1>
			where T0 : unmanaged
			where T1 : unmanaged
		{
			public int result;
			public T0 value0;
			public T1 value1;
		}
	}
}
