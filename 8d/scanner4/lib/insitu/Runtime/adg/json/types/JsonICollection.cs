namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Represents the object contains a collection of Json elements
		/// </summary>
		public interface ICollection
		{
			/// <summary>
			///  Add json element
			/// </summary>
			/// <param name="name">
			///  Token name
			/// </param>
			/// <param name="json">
			///  Token value
			/// </param>
			void Add(string name, Json json);
		}
	}
}
