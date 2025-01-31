using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;


namespace insitu
{
	public static partial class Vicon
	{
		/// <summary>
		///		Fetches data from Vicon.
		/// </summary>
		/// <remarks>
		///		When creating an instance make sure to check if DLL is not zero.
		///		When calling a Vicon internals, lock the worker instance, as it stops it from destroying the memory.
		/// </remarks>
		public sealed class Worker : IDisposable
		{
			public const double DefaultPositionScale = 0.01;

			/// <summary>
			///		Default transformation matrix to convert from Vicon transform space to Unity transform space.
			/// </summary>
			public static readonly double4x4 DefaultPositionTransform = new double4x4
			{
				m00 = 0                   , m01 = 0                    , m02 = -DefaultPositionScale, m03 = 0,
				m10 = DefaultPositionScale, m11 = 0                    , m12 = 0                    , m13 = 0,
				m20 = 0                   , m21 = -DefaultPositionScale, m22 = 0                    , m23 = 0,
				m30 = 0                   , m31 = 0                    , m32 = 0                    , m33 = 1.00,
			};

			/// <summary>
			///		Default rotation matrix to convert from Vicon vector space to Unity vector space.
			/// </summary>
			public static readonly double3x3 DefaultVectorTransform = new double3x3
			{
				m00 =  0, m01 =  0, m02 = -1,
				m10 =  1, m11 =  0, m12 =  0,
				m20 =  0, m21 = -1, m22 =  0,
			};

			/// <summary>
			///		Current state of the worker
			/// </summary>
			public State State;

			/// <summary>
			///		Gets called after State has been updated.
			///		This gets called in another thread.
			/// </summary>
			/// <remarks>
			///		When creating your own worker, this has to be called manually as it is not called in InternalUpdate.
			/// </remarks>
			public Action<State> OnState;

			/// <summary>
			///		Worker thread.
			/// </summary>
			public Thread Thread;
			
			/// <summary>
			///		Stores the last error from Tread.
			/// </summary>
			public string Error;

			/// <summary>
			///		When true the worker will perform the rescan instead of the caller.
			///		This greatly improves performance, but the scan is not immediately performed.
			/// </summary>
			/// <seealso cref="Rescan(bool)"/>
			public volatile bool QueueRescan;

			/// <summary>
			///		When true the worker will change the transformation matrix safely.
			///		This greatly improves performance, but the scan is not immediately performed.
			///		It is also possible to change the transformation matrix directly,
			///		but it can give unexpected results for a frame.
			/// </summary>
			/// <seealso cref="QueueTransformArg0"/>
			/// <seealso cref="QueueTransformArg1"/>
			/// <seealso cref="Transform(double4x4, bool)"/>
			public volatile bool QueueTransform;

			/// <see cref="Transform(double4x4, bool)"/>
			public double4x4 QueueTransformArg0;

			/// <see cref="Transform(double4x4, bool)"/>
			public double3 QueueTransformArg1;

			public readonly object Lock;
			public readonly IntPtr DLL;
			public readonly Stopwatch Stopwatch;
			public readonly Version SoftwareVersion;
			public readonly GCHandle GCHandle;
			public readonly string Host;
			public readonly int StreamMode;
			public readonly int ViconMode;

			public Worker(IntPtr dll, Stopwatch stopwatch, Version version, string host, int stream_mode, int vicon_mode)
			{
				var handle = GCHandle.Alloc(this, GCHandleType.Pinned);
				DLL = dll;
				Stopwatch = stopwatch;
				SoftwareVersion = version;
				GCHandle = handle;
				Host = host;
				StreamMode = stream_mode;
				ViconMode = vicon_mode;
				State.position_center = default;
				State.position_transform = DefaultPositionTransform;
				State.vector_transform = DefaultVectorTransform;
			}

			public Worker(string host, int stream_mode, int vicon_mode, Stopwatch stopwatch)
			{
				var handle = GCHandle.Alloc(this, GCHandleType.Pinned);
				var dll = ViconDLL.Client_Create();
				var version = VersionOf(dll);

				DLL = dll;
				Stopwatch = stopwatch;
				SoftwareVersion = version;
				GCHandle = handle;
				Host = host;
				StreamMode = stream_mode;
				ViconMode = vicon_mode;
				State.position_transform = DefaultPositionTransform;
				State.vector_transform = DefaultVectorTransform;
			}

			/// <summary>
			///		Perform a rescan of all the objects present in the Vicon data.
			/// </summary>
			/// <param name="delayed">
			///		When marked true the procedure improves greatly in performance, but the scan is not immediately performed.
			///		It is advised to mark this as true.
			/// </param>
			public void Rescan(bool delayed)
			{
				if (delayed)
				{
					QueueRescan = true;
				}
				else
				{
					lock (this)
					{
						Scan(DLL, ref State);
					}
				}
			}

			/// <summary>
			///		Overwrite the Vicon transform space to Unity transform space transformation matrix.
			/// </summary>
			/// <param name="delayed">
			///		When marked true the procedure improves greatly in performance, but the scan is not immediately performed.
			///		It is advised to mark this as true.
			/// </param>
			public void Transform(double4x4 transform, double3 center, bool delayed)
			{
				if (delayed)
				{
					QueueTransformArg0 = transform;
					QueueTransformArg1 = center;
					QueueTransform = true;
				}
				else
				{
					var rotation = double3x3.normalized(transform);
					lock (this)
					{
						State.position_transform = transform;
						State.position_center = center;
						State.vector_transform = rotation;
					}
				}
			}


			/// <summary>
			///		Start the worker thread.
			///		If you don't want to start another thread; first call InternalConnect and InternalSetup.
			///		Then call InternalUpdate on an interval.
			/// </summary>
			/// <remarks>
			///		DLL has to be valid, otherwise the procedure will do nothing.
			///		To test if the call was successful, monitor StateVersion until it is not 0.
			/// </remarks>
			public void Start()
			{
				if (DLL == IntPtr.Zero)
					return;

				State.version = 0;
				Thread = new Thread(InternalRun);
				Thread.Start(this);
			}

			/// <summary>
			///		Configure wireless connection to Vicon.
			///		The procedure will return ViconDLL.Success if success.
			///		Otherwise it will also store the error message in <paramref name="message"/>.
			/// </summary>
			public int ConfigureWireless(out string message)
			{
				if (DLL == IntPtr.Zero)
				{
					message = "DLL is not defined";
					return ViconDLL.ConfigurationFailed;
				}

				var ptr = Marshal.AllocHGlobal(512);
				var result = ViconDLL.Client_ConfigureWireless(DLL, 512, ptr);
				message = result == ViconDLL.Success ? null : Marshal.PtrToStringAnsi(ptr);
				Marshal.FreeHGlobal(ptr);

				return result;
			}

			/// <inheritdoc />
			public void Dispose()
			{
				State.version = -1;

				lock (this)
				{
					State.version = -1;
					if (GCHandle.IsAllocated)
						GCHandle.Free();

					if (DLL != IntPtr.Zero)
						ViconDLL.Client_Destroy(DLL);
				}
			}

			/// <summary>
			///		Tries to connect to Vicon host.
			///		returns 0 if successful.
			///		returns -1 if the call has failed and the worker is marked as dead; create a new worker if this is the case.
			///		returns 1 if the call has failed and stores the error in the worker Error field.
			/// </summary>
			/// <seealso cref="Error"/>
			public static int InternalConnect(Worker self, IntPtr dll)
			{
				var err = ViconDLL.Client_Connect(dll, self.Host);
				if (err == ViconDLL.Success)
					return 0;

				if (self.State.version < 0)
					return -1;

				var connected = ViconDLL.Client_IsConnected(dll);
				self.Error = $"Client_Connect failed connection to {self.Host} with error {err}. The IsConnected resulted: {connected}. Trying again..";
				return 1;
			}

			/// <summary>
			///		Enable all required data streams and setup State.
			/// </summary>
			public static void InternalSetup(Worker self, IntPtr dll)
			{
				ViconDLL.Client_SetStreamMode(dll, self.StreamMode);
				ViconDLL.Client_GetFrame(dll);
				if (ViconDLL.Client_EnableLightweightSegmentData(dll) != ViconDLL.Success)
					ViconDLL.Client_EnableSegmentData(dll);
				ViconDLL.Client_EnableMarkerData(dll);
				ViconDLL.Client_EnableUnlabeledMarkerData(dll);
				ViconDLL.Client_EnableDeviceData(dll);
				ViconDLL.Client_SetAxisMapping(dll, 4, 2, 0);
				ViconDLL.Client_GetFrame(dll);
				ViconDLL.Client_GetFrame(dll);

				Scan(dll, ref self.State);
				self.State.version = 1 | (int)(Hash.Simple((uint)dll, 14161351U) & 0x7FFF);
			}

			/// <summary>
			///		Executes queued tasks and updates the state.
			/// </summary>
			public static State InternalUpdate(Worker self, IntPtr dll, int vicon_mode)
			{
				Vicon.Fetch(dll);

				var state = self.State;
				if (self.QueueTransform)
				{
					self.QueueTransform = false;
					var transform = self.QueueTransformArg0;
					var center = self.QueueTransformArg1;

					var rotation = double3x3.normalized(transform);
					state.position_transform = transform;
					state.position_center = center;
					state.vector_transform = rotation;
				}
				if (self.QueueRescan)
				{
					Scan(dll, ref state);
					self.QueueRescan = false;
				}

				state.time = self.Stopwatch.Elapsed.TotalSeconds;
				if (Update(dll, ref state, vicon_mode))
					self.State.version++;

				self.State = state;
				return state;
			}

			/// <summary>
			///		Worker thread procedure.
			///		Calls InternalConnect and InternalSetup.
			///		Afterwards it will perform an infinite loop where it will execute InternalUpdate and OnState.
			///		The procedure will kill itself when State.version is less then 0.
			/// </summary>
			/// <param name="arg">The instance of Worker</param>
			public static void InternalRun(object arg)
			{
				var self = (Worker)arg;
				var dll = self.DLL;
				var vicon_mode = self.ViconMode;

				for (;;)
				{
					int result;
					lock (arg)
					{
						if (self.State.version < 0)
							return;

						result = InternalConnect(self, dll);
					}

					if (result == 0) break;
					if (result < 0) return;
					Thread.Sleep(200);
				}

				lock (arg)
				{
					if (self.State.version < 0)
						return;

					InternalSetup(self, dll);
				}

				for (;;)
				{
					State state;
					lock (arg)
					{
						if (self.State.version < 0)
							break;

						state = InternalUpdate(self, dll, vicon_mode);
					}

					if (self.OnState != null)
						self.OnState(state);
				}
			}
		}
	};
}
