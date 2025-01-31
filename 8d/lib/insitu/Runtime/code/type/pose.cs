using System.Runtime.InteropServices;


namespace insitu
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct pose
	{
		public double4 rotation;
		public double3 position;
		public int     rotation_valid;
		public int     position_valid;
	}
}
