using System;
using System.Runtime.InteropServices;


namespace insitu
{
	public static partial class Vicon
	{
		public sealed class Vusion : IDisposable
		{
			public const string dll = "HMDFusionUtils_Unity";

			public const int ESuccess = 10;
			public const int EQuaternionWasNan = 11;
			public const int EZeroTimeDelta = 12;
			public const int EInputIsIdentity = 13;
			public const int ELastRotationIdentity = 14;
			public const int ENoVelocity = 15;
			public const int EUninitialized = 16;


			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern void HighResTimer(out double pTime);

			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern double ScalarVelocity(ref double3 HmdOrientation);

			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr CreateFusionService();

			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern void DestroyFusionService(IntPtr dll);

			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern void Reset(IntPtr dll);

			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern int CalculateAngularVelocity(IntPtr dll, ref double4 i_R, double i_T, out double o_V);

			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern int UpdateOrientation2(IntPtr dll, double Time, ref double4 i_rHMDOrientation, double i_rHMDOrientationV, ref double4 i_rViconOrientation, bool i_bViconDataValid, out double4 pUpdatedOrientation);

			[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
			public static extern int UpdateOrientation3(IntPtr pService, double Time, ref double4 i_rHMDOrientation, bool i_bHMDDataValid, ref double4 i_rViconOrientation, uint i_ViconFrameNumber, bool i_bViconDataValid, float i_MaxAngularRateDegrees, uint i_WindowSize, out double4 pUpdatedOrientation);


			public readonly IntPtr DLL;
			public readonly GCHandle GCHandle;

			public Vusion()
			{
				var handle = GCHandle.Alloc(this, GCHandleType.Pinned);
				var dll = CreateFusionService();
				if (dll != IntPtr.Zero)
				{
					Reset(dll);
				}

				DLL = dll;
				GCHandle = handle;
			}

			public double4 Update(double4 vicon, bool vicon_valid, double4 gyro)
			{
				lock (this)
				{
					int err;
					HighResTimer(out var time);
					err = CalculateAngularVelocity(DLL, ref gyro, time, out var velocity);
					if (err != ESuccess)
						return vicon_valid ? vicon : gyro;

					double4 rotation;
					//err = UpdateOrientation3(DLL, time, ref gyro, true, ref vicon, vicon_frame, true, 1.0f, 20, out rotation);
					err = UpdateOrientation2(DLL, time, ref gyro, velocity, ref vicon, vicon_valid, out rotation);
					if (err != ESuccess)
						return vicon_valid ? vicon : gyro;

					return rotation;
				}
			}

			/// <inheritdoc />
			public void Dispose()
			{
				lock (this)
				{
					if (GCHandle.IsAllocated)
						GCHandle.Free();

					if (DLL != IntPtr.Zero)
					{
						Reset(DLL);
						DestroyFusionService(DLL);
					}
				}
			}
		}
	}
}
