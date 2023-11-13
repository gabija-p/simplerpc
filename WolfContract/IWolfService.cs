namespace Services;

/// <summary>
/// Rabbit descriptor
/// </summary>
public class RabbitDesc
{
	/// <summary>
	/// Rabbit ID.
	/// </summary>
	public int RabbitId { get; set; }

	/// <summary>
	/// Rabbit weight.
	/// </summary>
	public int Weight { get; set; }

	/// <summary>
	/// Rabbit's distance to wolf.
	/// </summary>
	public int DistanceToWolf { get; set; }
}
/// <summary>
/// Water descriptor.
/// </summary>
public class WaterDesc
{
	/// <summary>
	/// Body of water ID.
	/// </summary>
	public int WaterId { get; set; }

	/// <summary>
	/// Water location's coordinate X.
	/// </summary>
	public int CoordinateX { get; set; }

	/// <summary>
	/// Water location's coordinate Y.
	/// </summary>
	public int CoordinateY { get; set; }

	/// <summary>
	/// Amount of water in the body of water
	/// </summary>
	public int Volume { get; set; }
}

/// <summary>
/// Rabbit state.
/// </summary>
public enum RabbitState : int
{
	Eaten,
	NotEaten,
	SeenButNotEaten
}

/// <summary>
/// Water state.
/// </summary>
public enum WaterState : int
{
	Drunk,
	NotDrunk,
	SeenButNotDrunk
}


/// <summary>
/// Service contract.
/// </summary>
public interface IWolfService
{
	/// <summary>
	/// Get next unique ID from the server. Is used by rabbits and water to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	int GetUniqueId();

	/// <summary>
	/// Check if the wolf ate the rabbit
	/// </summary>
	/// <param name="rabbit">Rabbit descriptor.</param>
	/// <returns>Rabbit state that defines whether or not the rabbit was eaten</returns>
	RabbitState DidEat(RabbitDesc rabbit);

	/// <summary>
	/// Check if the wolf drank the water
	/// </summary>
	/// <param name="water"></param>
	/// <returns>Water state that defines whether or not the water was drunk</returns>
	WaterState DidDrink(WaterDesc water);
}
