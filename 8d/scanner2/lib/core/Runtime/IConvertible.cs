namespace ADG
{
	/// <summary>
	///  Mark the object it can be converted to the specified type
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IConvertible<out T>
	{
		/// <summary>
		///  Object's value of type <typeparamref name="T" />
		/// </summary>
		T Value { get; }
	}
}
