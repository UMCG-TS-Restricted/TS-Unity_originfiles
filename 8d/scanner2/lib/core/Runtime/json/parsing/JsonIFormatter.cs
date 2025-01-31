using System.Text;


namespace ADG
{
	public partial class Json
	{
		/// <summary>
		///  Marks the object as having a custom serializer.
		///  This will override its default serialization.
		/// </summary>
		public interface IFormatter
		{
			/// <summary>
			///  Stringifies to Json
			/// </summary>
			/// <see cref="Json.Stringify(object, StringBuilder, Formatter, int)" />
			/// <seealso cref="JsonUtilities.Stringify"/>
			StringBuilder Stringify(
				StringBuilder builder,
				Formatter formatter,
				int indent = 0);
		}
	}
}
