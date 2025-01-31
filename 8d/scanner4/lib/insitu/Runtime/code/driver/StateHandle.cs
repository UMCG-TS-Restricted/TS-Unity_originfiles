namespace insitu
{
	public struct StateHandle
	{
		public int version;
		public int index;
		public string name;

		public static implicit operator StateHandle(string value) => new StateHandle
		{
			version = 0,
			index = 0,
			name = value,
		};

		public readonly StateHandle Update(string new_name)
		{
			if (string.Equals(name, new_name))
				return this;

			return new StateHandle
			{
				version = 0,
				index = 0,
				name = new_name,
			};
		}
	}
}
