using System.Collections.Generic;
using System.Text;


namespace ADG
{
	public partial class Json
	{
		public struct Parser
		{
			public StringBuilder Cache;
			public Stack<ICollection> Stack;

			public static Parser Create()
			{
				return new Parser
				{
					Cache = new StringBuilder(1024),
					Stack = new Stack<ICollection>(24),
				};
			}

			public Json Parse(string text) => Json.Parse(text, Cache, Stack);
			public Object ParseObject(string text) => Json.ParseObject(text, Cache, Stack);
			public Array ParseArray(string text) => Json.ParseArray(text, Cache, Stack);
		}
	}
}
