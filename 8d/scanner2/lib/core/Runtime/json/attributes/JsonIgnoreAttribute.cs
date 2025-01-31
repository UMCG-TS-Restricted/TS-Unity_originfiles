using System;


namespace ADG
{
	/// <summary>
	///  Prevent field or attribute from parsing to JSON
	/// </summary>
	/// <inheritdoc cref="Attribute" />
	/// <inheritdoc cref="IIntrospectAttribute" />
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class JsonIgnoreAttribute : Attribute { }
}
