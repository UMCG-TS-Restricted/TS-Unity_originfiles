using System;


namespace ADG
{
	/// <summary>
	///  Mark as JSON serializable
	/// </summary>
	/// <inheritdoc cref="Attribute" />
	/// <inheritdoc cref="IIntrospectAttribute" />
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class JsonAttribute : Attribute
	{
		/// <summary>
		///  Override member name used for serialization
		/// </summary>
		public readonly string Name;

		/// <param name="name">
		///  Name of the field in JSON
		///  Leave null for the field name itself
		/// </param>
		/// <inheritdoc />
		public JsonAttribute(string name = null)
		{
			Name = name;
		}
	}
}
