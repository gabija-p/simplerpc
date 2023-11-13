namespace Servers;

using Services;

/// <summary>
/// Service
/// </summary>
public class WolfService : IWolfService
{
	//NOTE: non-singleton service would need logic to be static or injected from a singleton instance
	private readonly WolfLogic mLogic = new WolfLogic();

	/// <summary>
	/// Get next unique ID from the server. Is used by cars to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	public int GetUniqueId() 
	{
		return mLogic.GetUniqueId();
	}

	/// <summary>
	/// Check if the wolf ate the rabbit
	/// </summary>
	/// <param name="rabbit">Rabbit descriptos.</param>
	/// <returns>Rabbit state that defines whether or not the rabbit was eaten</returns>
	public RabbitState DidEat(RabbitDesc rabbit)
	{
		return mLogic.DidEat(rabbit);
	}
	
	/// <summary>
	/// Check if the wolf drank the water
	/// </summary>
	/// <param name="water"></param>
	/// <returns>Water state that defines whether or not the water was drunk</returns>
	public WaterState DidDrink(WaterDesc water)
	{
		return mLogic.DidDrink(water);
	}
}