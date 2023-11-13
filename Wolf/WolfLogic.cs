namespace Servers;

using NLog;

using Services;

/// <summary>
/// Wolf state descritor.
/// </summary>
public class WolfState
{
	/// <summary>
	/// Access lock.
	/// </summary>
	public readonly object AccessLock = new object();

	/// <summary>
	/// Rabbit state.
	/// </summary>
	public RabbitState RabbitState;

	/// <summary>
	/// Water state.
	/// </summary>
	public WaterState WaterState;

	/// <summary>
	/// Last unique ID value generated.
	/// </summary>
	public int LastUniqueId;

	/// <summary>
	/// Wolf's current location in coordinate x
	/// </summary>
	public int CoordinateX;

	/// <summary>
	/// Wolf's current location in coordinate y
	/// </summary>
	public int CoordinateY;

	/// <summary>
	/// If a rabbit or a body of water is closer than this distance, they will be consumed
	/// </summary>
	public int SafeDistanceToWolf = 5;

	/// <summary>
	/// The amount of food and water the wolf has currently consumed
	/// </summary>
	public int WolfFoodAmount;

	/// <summary>
	/// The amount of food and water the wolf can consume in total, before having to rest and digest
	/// </summary>
	public int WolfMaxFoodAmount = 100;
}


/// <summary>
/// <para>Wolf logic.</para>
/// <para>Thread safe.</para>
/// </summary>
class WolfLogic
{
	/// <summary>
	/// Logger for this class.
	/// </summary>
	private Logger mLog = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Background task thread.
	/// </summary>
	private Thread mBgTaskThread;

	/// <summary>
	/// State descriptor.
	/// </summary>
	private WolfState mState = new WolfState();
	

	/// <summary>
	/// Constructor.
	/// </summary>
	public WolfLogic()
	{
		//start the background task
		mBgTaskThread = new Thread(BackgroundTask);
		mBgTaskThread.Start();
	}

	/// <summary>
	/// Get next unique ID from the server. Is used by rabbits and water to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	public int GetUniqueId() 
	{
		lock( mState.AccessLock )
		{
			mState.LastUniqueId += 1;
			return mState.LastUniqueId;
		}
	}

	/// <summary>
	/// Check if the wolf ate the rabbit
	/// </summary>
	/// <returns>Rabbit state that defines whether or not the rabbit was eaten</returns>
	public RabbitState DidEat(RabbitDesc rabbit) 
	{
		lock( mState.AccessLock )
		{
			if(rabbit.DistanceToWolf < mState.SafeDistanceToWolf)
			{
				if(mState.WolfFoodAmount >= mState.WolfMaxFoodAmount)
				{
					mLog.Info($"I saw a rabbit, id:{rabbit.RabbitId} but was too full to catch it.");

					return RabbitState.SeenButNotEaten;
				}
				mState.WolfFoodAmount += rabbit.Weight;
				mLog.Info($"I ate a rabbit, id:{rabbit.RabbitId}. My satiation scale is: {mState.WolfFoodAmount}/{mState.WolfMaxFoodAmount}");
				return RabbitState.Eaten;
			}
			else 
			{
				return RabbitState.NotEaten;
			}
		}
	}

	/// <summary>
	/// Check if the wolf drank the water
	/// </summary>
	/// <param name="water"></param>
	/// <returns>Water state that defines whether or not the water was drunk</returns>
	public WaterState DidDrink(WaterDesc water)
	{
		mLog.Info("I'm searching for water");
		lock(mState.AccessLock)
		{
			//mLog.Info($"wax: {water.CoordinateX} way: {water.CoordinateY}, wx: {mState.CoordinateX} wy: {mState.CoordinateY})");
			var distanceToWater = Math.Sqrt(Math.Pow(mState.CoordinateX - water.CoordinateX, 2) + 
				Math.Pow(mState.CoordinateY - water.CoordinateY, 2));
			mLog.Info($"Distance to water id: {water.WaterId} is: {distanceToWater}");

			if(distanceToWater < mState.SafeDistanceToWolf)
			{
				if(mState.WolfFoodAmount >= mState.WolfMaxFoodAmount)
				{
					mLog.Info($"I saw a body of water id:{water.WaterId} but was too full to drink it.");
					return WaterState.SeenButNotDrunk;
				}
				else
				{
					mState.WolfFoodAmount += water.Volume;
					mLog.Info($"I drank a body of water id:{water.WaterId}. My satiation scale is: {mState.WolfFoodAmount}/{mState.WolfMaxFoodAmount}");
					return WaterState.Drunk;
				}
			}
			else
			{
				mLog.Info($"Water id: {water.WaterId} was too far");
				return WaterState.NotDrunk;
			}
		}
	}

	/// <summary>
	/// Background task for the wolf.
	/// </summary>
	public void BackgroundTask()
	{
		Console.Title = "Wolf";

		//intialize random number generator
		var rnd = new Random();

		//wait for the server to load completely
		Thread.Sleep(500);

		//generate wolf's initial location
		mState.CoordinateX = rnd.Next(0, 30);
		mState.CoordinateY = rnd.Next(0, 30);

		mLog.Info($"The wolf appeared at coordinates x: {mState.CoordinateX} y: {mState.CoordinateY}.");

		while( true )
		{
			var WolfIsFull = false;

			lock( mState.AccessLock )
			{
				if(mState.WolfFoodAmount >= mState.WolfMaxFoodAmount)
				{
					WolfIsFull = true;
					mLog.Info("The wolf will now take 5 seconds to digest.");
				}
			}

			if(WolfIsFull)
			{
				Thread.Sleep(5000);
			}

			lock( mState.AccessLock )
			{
				//generate wolf's next location		
				mState.CoordinateX = rnd.Next(0, 30);
				mState.CoordinateY = rnd.Next(0, 30);
				if(WolfIsFull)
				{
					mState.WolfFoodAmount = 0;
					WolfIsFull = false;
				}

			}

			mLog.Info($"The wolf moved to coordinates x: {mState.CoordinateX} y: {mState.CoordinateY}.");

			//wolf stays in one position for a while before moving again
			Thread.Sleep(5000);
		}
	}
}