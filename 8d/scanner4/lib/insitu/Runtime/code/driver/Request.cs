using System.Runtime.InteropServices;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		///		Vicon DLL works with structures that return a result code and the actual result.
		///		This functions as a wrapper.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Request<T> where T : unmanaged
		{
			public int result;
			public T value;
		}

		/// <summary>
		///		Vicon DLL works with structures that return a result code and the actual result.
		///		This functions as a wrapper with two result values.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Request<T0, T1> where T0 : unmanaged where T1 : unmanaged
		{
			public int result;
			public T0 value0;
			public T1 value1;
		}
	}
}
