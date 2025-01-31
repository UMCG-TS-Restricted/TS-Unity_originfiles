using System;


namespace ADG
{

	[Serializable]
	public struct StringReference
	{
		public SharedString Shared;
		public string Self;
		public bool UseShared;

		public string Value => UseShared && Shared ? Shared.Value : Self;

		public StringReference(string value)
		{
			Shared = null;
			Self = value;
			UseShared = false;
		}

		public StringReference(SharedString value)
		{
			Shared = value;
			Self = null;
			UseShared = true;
		}
	}
}
