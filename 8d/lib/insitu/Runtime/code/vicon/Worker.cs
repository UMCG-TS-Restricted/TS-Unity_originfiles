using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		///		When creating an instance make sure to check if DLL is not zero.
		/// </remarks>
		public sealed class Worker : IDisposable
		{
			public State State;
			public volatile int StateVersion;
			public int FrameCount;

			public Thread Thread;
			public string Error;

			public readonly IntPtr DLL;
			public readonly Stopwatch Stopwatch;
			public readonly Version SoftwareVersion;
			public readonly GCHandle GCHandle;
			public readonly string Host;
			public readonly int StreamMode;

			public Worker(IntPtr dll, Stopwatch stopwatch, Version version, string host, int stream_mode)
			{
				var handle = GCHandle.Alloc(this, GCHandleType.Pinned);
				DLL = dll;
				Stopwatch = stopwatch;
				SoftwareVersion = version;
				GCHandle = handle;
				Host = host;
				StreamMode = stream_mode;
			}

			public Worker(string host, int stream_mode)
			{
				var handle = GCHandle.Alloc(this, GCHandleType.Pinned);
				var dll = Client_Create();
				var version = VersionOf(dll);
				var stopwatch = Stopwatch.StartNew();

				DLL = dll;
				Stopwatch = stopwatch;
				SoftwareVersion = version;
				GCHandle = handle;
				Host = host;
				StreamMode = stream_mode;
			}


			/// <summary>
			/// 
			/// </summary>
			/// <remarks>
			///		To test if the call was successful, monitor StateVersion until it is not 0.
			/// </remarks>
			public void Start()
			{
				if (DLL == IntPtr.Zero)
					return;

				StateVersion = 0;
				Thread = new Thread(InternalRun);
				Thread.Start(this);
			}

			public int ConfigureWireless(out string message)
			{
				if (DLL == IntPtr.Zero)
				{
					message = "DLL is not defined";
					return ConfigurationFailed;
				}

				var ptr = Marshal.AllocHGlobal(512);
				var result = Client_ConfigureWireless(DLL, 512, ptr);
				message = result == Success ? null : Marshal.PtrToStringAnsi(ptr);
				Marshal.FreeHGlobal(ptr);

				return result;
			}

			public void Scan()
			{
				lock (this)
				{
					Vicon.Scan(DLL, ref State);
					StateVersion++;
				}
			}

			/// <inheritdoc />
			public void Dispose()
			{
				StateVersion = -1;

				lock (this)
				{
					StateVersion = -1;
					if (GCHandle.IsAllocated)
						GCHandle.Free();

					if (DLL != IntPtr.Zero)
						Client_Destroy(DLL);
				}
			}

			public static void Dispose(List<GCHandle> handles)
			{
				if (handles == null)
					return;

				for (var i = 0; i < handles.Count; i++)
				{
					var gch = handles[i];
					gch.Free();
				}

				handles.Clear();
			}

			public static void InternalRun(object arg)
			{
				var self = (Worker)arg;
				var dll = self.DLL;


				// Connecting
				for (;;)
				{
					if (self.StateVersion < 0)
						goto __cleanup;

					lock (arg)
					{
						var err = Client_Connect(dll, self.Host);
						if (err == Success)
							break;

						var connected = Client_IsConnected(dll);
						self.Error = $"Client_Connect failed connection to {self.Host} with error {err}. The IsConnected resulted: {connected}. Trying again..";
					}

					Thread.Sleep(200);
				}

				// Setup
				lock (arg)
				{
					if (self.StateVersion < 0)
						goto __cleanup;

					Client_SetStreamMode(dll, self.StreamMode);
					Client_GetFrame(dll);
					if (Client_EnableLightweightSegmentData(dll) != Success)
						Client_EnableSegmentData(dll);
					Client_EnableMarkerData(dll);
					Client_EnableUnlabeledMarkerData(dll);
					Client_EnableDeviceData(dll);
					Client_SetAxisMapping(dll, 4, 2, 0);
					Client_GetFrame(dll);
					Client_GetFrame(dll);
					Client_GetFrame(dll);

					Vicon.Scan(dll, ref self.State);
					self.StateVersion = 1 | (int)(Hash.Simple((uint)dll, 14161351U) & 0x7FFF);
				}

				// Update loop
				for (;;)
				{
					if (self.StateVersion < 0)
						goto __cleanup;

					lock (arg)
					{
						if (Update(dll, ref self.State, out self.FrameCount))
							self.StateVersion++;
					}
				}

				__cleanup: {
					// Cleanup
				};
			}
		}
	};
}
