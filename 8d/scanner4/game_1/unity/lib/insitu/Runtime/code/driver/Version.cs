using System;
using System.Runtime.InteropServices;
using System.Security;

namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		///		Version notation of major.minor.point used in Vicon software.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Version
		{
			public uint major;
			public uint minor;
			public uint point;

			public override string ToString() => $"{major}.{minor}.{point}";
		}

		/// <summary>
		///		Query software version of Vicon.
		/// </summary>
		[SecurityCritical]
		public static unsafe Version VersionOf(IntPtr dll)
		{
			if (dll == IntPtr.Zero)
				return new Version();

			Version version = new Version { };
			ViconDLL.Client_GetVersion(dll, (IntPtr)(&version));
			return version;
		}
	}
}
