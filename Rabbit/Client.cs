namespace Clients;

using Microsoft.Extensions.DependencyInjection;

using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;

using NLog;

using Services;

class Client
{
	/// <summary>
	/// Logger for this class.
	/// </summary>
	Logger mLog = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Configures logging subsystem.
	/// </summary>
	private void ConfigureLogging()
	{
		var config = new NLog.Config.LoggingConfiguration();

		var console =
			new NLog.Targets.ConsoleTarget("console")
			{
				Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
			};
		config.AddTarget(console);
		config.AddRuleForAllLevels(console);

		LogManager.Configuration = config;
	}

	/// <summary>
	/// Program body.
	/// </summary>
	private void Run() {
		//configure logging
		ConfigureLogging();

		//initialize random number generator
		var rnd = new Random();

		//run everything in a loop to recover from connection errors
		while( true )
		{
			try {
				//connect to the server, get service client proxy
				var sc = new ServiceCollection();
				sc
					.AddSimpleRpcClient(
						"wolfService", //must be same as on line 65
						new HttpClientTransportOptions
						{
							Url = "http://127.0.0.1:5000/simplerpc",
							Serializer = "HyperionMessageSerializer"
						}
					)
					.AddSimpleRpcHyperionSerializer();

				sc.AddSimpleRpcProxy<IWolfService>("wolfService"); //must be same as on line 56

				var sp = sc.BuildServiceProvider();

				var wolf = sp.GetService<IWolfService>();

				//initialize rabbit descriptor
				var rabbit = new RabbitDesc();

				var isRabbitEaten = false;

				//get unique client id
				rabbit.RabbitId = wolf.GetUniqueId();	

				//generate rabbit weight
				rabbit.Weight = rnd.Next(1, 20);

				//log identity data
				mLog.Info($"A rabbit is born. It's id is: {rabbit.RabbitId}, it's weight is: {rabbit.Weight}.");
				Console.Title = $"Rabbit {rabbit.RabbitId}";
					
				//do the rabbit stuff
				while( true )
				{
					if(isRabbitEaten)
					{
						rabbit.Weight = rnd.Next(1, 20);
						mLog.Info($"A rabbit is born. It's id is: {rabbit.RabbitId}, it's weight is: {rabbit.Weight}.");
						isRabbitEaten = false;
					}

					rabbit.DistanceToWolf = rnd.Next(0, 30);
					
					mLog.Info($"I hopped, my distance to wolf is: {rabbit.DistanceToWolf}.");

					RabbitState didEat;

					//try to pass the wolf undetected
					didEat = wolf.DidEat(rabbit);
					
					if( didEat == RabbitState.NotEaten )
					{
						mLog.Info("I was undetected.");
						Thread.Sleep(500);
					}
					else if( didEat == RabbitState.Eaten )
					{
						mLog.Info("The rabbit was eaten.");
						isRabbitEaten = true;
						Thread.Sleep(5000);
					}
					else
					{
						mLog.Info("I was detected, but managed to run away.");
						continue;
					}
				}				
			}
			catch( Exception e )
			{
				//log whatever exception to console
				mLog.Warn(e, "Unhandled exception caught. Will restart main loop.");

				//prevent console spamming
				Thread.Sleep(2000);
			}
		}
	}

	/// <summary>
	/// Program entry point.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	static void Main(string[] args)
	{
		var self = new Client();
		self.Run();
	}
}
