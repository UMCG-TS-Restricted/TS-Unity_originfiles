using System;
using System.Runtime.InteropServices;


namespace insitu
{
	/// <summary>
	///		Bindings of ViconDataStreamSDK_C.
	///		See Vicon documentation for details.
	/// </summary>
	public static class ViconDLL
	{
		public const string DLL = "ViconDataStreamSDK_C";

		// Custom
		public const int ViconNexus = 1;
		public const int ViconTracker = 2;


		// Result
		public const int Unknown = 0;
		public const int NotImplemented = 1;
		public const int Success = 2;
		public const int InvalidHostName = 3;
		public const int InvalidMulticastIP = 4;
		public const int ClientAlreadyConnected = 5;
		public const int ClientConnectionFailed = 6;
		public const int ServerAlreadyTransmittingMulticast = 7;
		public const int ServerNotTransmittingMulticast = 8;
		public const int NotConnected = 9;
		public const int NoFrame = 10;
		public const int InvalidIndex = 11;
		public const int InvalidCameraName = 12;
		public const int InvalidSubjectName = 13;
		public const int InvalidSegmentName = 14;
		public const int InvalidMarkerName = 15;
		public const int InvalidDeviceName = 16;
		public const int InvalidDeviceOutputName = 17;
		public const int InvalidLatencySampleName = 18;
		public const int CoLinearAxes = 19;
		public const int LeftHandedAxes = 20;
		public const int HapticAlreadySet = 21;
		public const int EarlyDataRequested = 22;
		public const int LateDataRequested = 23;
		public const int InvalidOperation = 24;
		public const int NotSupported = 25;
		public const int ConfigurationFailed = 26;
		public const int NotPresent = 2;


		// StreamMode
		public const int ClientPull = 0;
		public const int ClientPullPreFetch = 1;
		public const int ServerPush = 2;


		// Direction
		public const int Up = 0;
		public const int Down = 1;
		public const int Left = 2;
		public const int Right = 3;
		public const int Forward = 4;
		public const int Backward = 5;


		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr Client_Create();

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_Destroy(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetVersion(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_Connect(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string HostName);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsConnected(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetSubjectName(IntPtr client, uint SubjectIndex, int sizeOfBuffer, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentGlobalRotationQuaternion(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_ConnectToMulticast(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string LocalIP, [MarshalAs(UnmanagedType.LPStr)] string MulticastIP);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_Disconnect(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_StartTransmittingMulticast(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string ServerIP, [MarshalAs(UnmanagedType.LPStr)] string MulticastIP);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_StopTransmittingMulticast(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableSegmentData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableLightweightSegmentData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableLightweightSegmentData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableMarkerData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableUnlabeledMarkerData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableDeviceData(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableSegmentData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableMarkerData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableUnlabeledMarkerData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableDeviceData(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsSegmentDataEnabled(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsLightweightSegmentDataEnabled(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsMarkerDataEnabled(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsUnlabeledMarkerDataEnabled(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsDeviceDataEnabled(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_SetStreamMode(IntPtr client, int Mode);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_SetApexDeviceFeedback(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rDeviceName, bool i_bOn);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]

		public static extern int Client_SetAxisMapping(IntPtr client, int XAxis, int YAxis, int ZAxis);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetAxisMapping(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetFrame(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetFrameNumber(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetTimecode(IntPtr client, IntPtr outptr); /*does not work*/

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetFrameRate(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetLatencySampleCount(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetLatencySampleName(IntPtr client, uint LatencySampleIndex, int sizeOfBuffer, IntPtr outstr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetLatencySampleValue(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string LatencySampleName,
																IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetLatencyTotal(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSubjectCount(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetSubjectRootSegmentName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, int sizeOfBuffer, IntPtr outstr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentCount(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetSegmentName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, uint SegmentIndex, uint sizeOfBuffer, IntPtr outstr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentChildCount(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetSegmentChildName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, uint SegmentIndex, int sizeOffBuffer, IntPtr outstr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetSegmentParentName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, int sizeOffBuffer, IntPtr outstr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentStaticTranslation(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentStaticRotationHelical(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentStaticRotationMatrix(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentStaticRotationQuaternion(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentStaticRotationEulerXYZ(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentStaticScale(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentGlobalTranslation(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentGlobalRotationHelical(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentGlobalRotationMatrix(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentGlobalRotationEulerXYZ(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentLocalTranslation(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentLocalRotationHelical(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentLocalRotationMatrix(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentLocalRotationQuaternion(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetSegmentLocalRotationEulerXYZ(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string SegmentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetMarkerCount(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetMarkerName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, uint MarkerIndex, int sizeOffBuffer, IntPtr outstr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetMarkerParentName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string MarkerName, int sizeOffBuffer, IntPtr outstr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetMarkerGlobalTranslation(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string MarkerName, IntPtr outptr);


		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetUnlabeledMarkerCount(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetUnlabeledMarkerGlobalTranslation(IntPtr client, uint MarkerIndex, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceCount(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetDeviceName(IntPtr client, uint DeviceIndex, int sizeOfBuffer, IntPtr outstr, ref int dtype); // dtype: enum ViconDataStreamSDK.CSharp.DeviceType

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceOutputCount(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetDeviceOutputName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, uint DeviceOutputIndex, int sizeOfBuffer, IntPtr outstr, ref int dUnit);


		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceOutputValue(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceOutputSubsamples(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceOutputValueForSubsample(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputName, uint Subsample, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetDeviceOutputComponentName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, uint DeviceOutputIndex, int sizeOfOutputBuffer, IntPtr OutputOutstr, int sizeOfComponentBuffer, IntPtr ComponentOutstr, ref int DeviceOutputUnit);



		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceOutputComponentValue(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputComponentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceOutputComponentSubsamples(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputComponentName, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetDeviceOutputComponentValueForSubsample(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string DeviceName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputName, [MarshalAs(UnmanagedType.LPStr)] string DeviceOutputComponentName, uint Subsample, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetForcePlateCount(IntPtr client, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGlobalForceVector(IntPtr client, uint ForcePlateIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGlobalMomentVector(IntPtr client, uint ForcePlateIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGlobalCentreOfPressure(IntPtr client, uint ForcePlateIndex, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetForcePlateSubsamples(IntPtr client, uint ForcePlateIndex, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGlobalForceVectorForSubsample(IntPtr client, uint ForcePlateIndex, uint Subsample, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGlobalMomentVectorForSubsample(IntPtr client, uint ForcePlateIndex, uint Subsample, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGlobalCentreOfPressureForSubsample(IntPtr client, uint ForcePlateIndex, uint Subsample, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetEyeTrackerCount(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetEyeTrackerGlobalPosition(IntPtr client, uint EyeTrackerIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetEyeTrackerGlobalGazeVector(IntPtr client, uint EyeTrackerIndex, IntPtr outptr);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetCameraUserId(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableGreyscaleData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsGreyscaleDataEnabled(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsVideoDataEnabled(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetFrameRateCount(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_SetCameraFilter(IntPtr client, UIntPtr i_rCameraIdsForCentroids, int i_numOfCentroidIds, UIntPtr i_rCameraIdsForBlobs, int i_numOfBlobIds);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetIsVideoCamera(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetCameraCount(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetLabeledMarkerCount(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsMarkerRayDataEnabled(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetObjectQuality(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string ObjectName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGreyscaleBlobCount(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsCentroidDataEnabled(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableDebugData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetFrameRateValue(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string FrameRateName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetMarkerRayContribution(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string MarkerName, uint MarkerRayContributionIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetCentroidCount(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableMarkerRayData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetCameraType(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, int sizeOfBuffer, IntPtr outstr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetGreyscaleBlob(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, uint i_BlobIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetServerOrientation(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetCameraDisplayName(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, int sizeOfBuffer, IntPtr outstr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableGreyscaleData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetCameraId(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetHardwareFrameNumber(IntPtr client, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableCentroidData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_IsDebugDataEnabled(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Client_SetBufferSize(IntPtr client, uint bufferSize);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetCentroidWeight(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, uint i_CentroidIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetFrameRateName(IntPtr client, uint FrameRateIndex, int sizeOfBuffer, IntPtr outstr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetCentroidPosition(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, uint i_CentroidIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_GetCameraName(IntPtr client, uint i_CameraIndex, int sizeOfBuffer, IntPtr outstr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableMarkerRayData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetLabeledMarkerGlobalTranslation(IntPtr client, uint MarkerIndex, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetCameraResolution(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rCameraName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_DisableDebugData(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Client_GetMarkerRayContributionCount(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string SubjectName, [MarshalAs(UnmanagedType.LPStr)] string MarkerName, IntPtr outptr);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_EnableCentroidData(IntPtr client);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_ClearSubjectFilter(IntPtr client);
		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_AddToSubjectFilter(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rSubjectName);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_SetTimingLogFile(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string i_rClientLog, [MarshalAs(UnmanagedType.LPStr)] string i_rStreamLog);

		[DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Client_ConfigureWireless(IntPtr client, int sizeOffBuffer, IntPtr outstr);
	}
}
