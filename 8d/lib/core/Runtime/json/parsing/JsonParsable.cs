using System.Collections.Generic;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		public interface IParsable<out T> where T : Json
		{
			T ToJson();
		}

		public static Array Parse<T>(IList<T> list) where T : IParsable<Json>
		{
			if (list == null)
				return null;

			var length = list.Count;
			var result = new Array();
			for (var i = 0; i < length; i++)
			{
				var element = list[i];
				result.Add(element == null ? null : element.ToJson());
			}

			return result;
		}
	}
}
