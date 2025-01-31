
using System;
using System.Runtime.InteropServices;
using ADG;

namespace insitu
{
	[StructLayout(LayoutKind.Sequential, Pack = 1), Obsolete]
	public struct Timecode
	{
		public uint hours;
		public uint minutes;
		public uint seconds;
		public uint frames;
		public uint subframe;
		public bool field_flag;
		/// <summary>
		///  None = 0,
		///  PAL = 1,
		///  NTSC = 2,
		///  NTSCDrop = 3,
		///  Film = 4,
		///  NTSCFilm = 5,
		///  ATSC = 6,
		/// </summary>
		public int standard;
		public uint subframes_per_frame;
		public uint user_bits;


		public Json.Object ToJson() => new Json.Object
		{
			{"hours", hours },
			{"minutes", minutes},
			{"seconds", seconds},
			{"frames", frames},
			{"subframe", subframe},
			{"field_flag", field_flag },
			{"standard", standard},
			{"subframes_per_frame", subframes_per_frame },
			{"user_bits", user_bits},
		};
	}
}