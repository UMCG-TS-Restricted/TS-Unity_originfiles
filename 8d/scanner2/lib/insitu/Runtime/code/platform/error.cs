namespace insitu
{
	/// <summary>
	///		Error messages stored in a single file to be easily edited.
	/// </summary>
	public struct error
	{
		public static readonly string AppNotAssigned = "App is not assigned: assignment of App is necessary to retrieve Vicon data, without this the object does not work properly.";
		public static readonly string WorkerIsNull = "Vicon worker not started!";
		public static readonly string StateIsDead = "Worker has stopped..";
		public static readonly string StateIsNull = "Waiting for worker..";
		public static readonly string FileIsNull = "No recording has been opened";
		public static readonly string DeviceNotFound = "Device not found";

		public static readonly string AssertNoBody = "Calling write body without the object has been marked to have a bode. Make sure to call Begin before a write call and make sure to have the FlagBody on.";
		public static readonly string InvalidFileTypeId = "Invalid file! File should start with the FileDescriptor Header";
		public static readonly string JsonParseFailed = "Json data was found, but failed to parse.";

		public static string FieldIsNull(string field) => $"{field} is not assigned!";
		public static string InvalidPose(string field) => $"{field} has not a valid pose!";

		public static string TypeNotFound(ushort type) => $"{type:X} has not been found. Make sure to register the type.";
		public static string TypeOfFieldNotFound(ushort type, string name) => $"{type:X} has not been found, but is required by field {name}. Make sure to register the type.";
		public static string MetadataChange(string previous, string current) => $"Metadata was found again. Replacing {previous} with {current}.";
	}
}
