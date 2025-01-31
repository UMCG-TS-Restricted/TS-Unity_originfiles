using System.Runtime.InteropServices;
using ADG;


namespace insitu
{
	/// <summary>
	///		Position and rotation + validity checks.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct pose
	{
		public double4    rotation;
		public double3    position;

		/// <summary>
		///		When the value equals 0, the rotation value is not valid.
		///		Otherwise it is valid and is safe to use.
		/// </summary>
		/// <remarks>This is a byte type instead of a bool as it helps with serializing the data.</remarks>
		public byte valid_rotation;

		/// <summary>
		///		When the value equals 0, the position value is not valid.
		///		Otherwise it is valid and is safe to use.
		/// </summary>
		/// <remarks>This is a byte type instead of a bool as it helps with serializing the data.</remarks>
		public byte valid_position;

		/// <summary>
		///		A zero value pose, except for the rotation value.
		///		The rotation is set to identity, to make sure you can safely use the rotation value without needing to create branches.
		/// </summary>
		/// <seealso cref="double4.identity"/>
		public static pose identity => new pose
		{
			rotation = double4.identity,
		};

		/// <summary>
		///		Convert the data to JSON.
		/// </summary>
		public Json.Object json() => new Json.Object
		{
			{ "rotation", rotation.json() },
			{ "position", position.json() },
			{ "valid_rotation", valid_rotation },
			{ "valid_position", valid_position },
		};

		/// <summary>
		///		Tries to convert JSON to a pose.
		/// </summary>
		public static pose from(Json.Object data, pose fallback)
		{
			var result = fallback;
			if (data == null)
				return result;

			result.rotation = double4.from(data.ArrayOf("rotation"), 0, double4.identity);
			result.position = double3.from(data.ArrayOf("position"));
			result.valid_rotation = data["valid_rotation"];
			result.valid_position = data["valid_position"];
			return result;
		}
	}
}
