namespace insitu
{
	/// <summary>
	///		An extension of IPose which explicitly gathers the Vicon data instead of Unity data.
	/// </summary>
	public interface IPoseSource
	{
		public pose PoseSource();
	}
}
