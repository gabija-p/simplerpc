﻿namespace Clients;

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

				//initialize water descriptor
				var water = new WaterDesc();

				//get unique client id
				water.WaterId = wolf.GetUniqueId();

				water.CoordinateX = rnd.Next(0, 30);
				water.CoordinateY = rnd.Next(0, 30);
				water.Volume = rnd.Next(1, 20);

				//log identity data
				mLog.Info($"I am body of water id: {water.WaterId}, My coordinates are x: {water.CoordinateX}, y: {water.CoordinateY}, volume: {water.Volume}.");
				Console.Title = $"Water id: {water.WaterId}.";
					
				//do the water stuff
				while( true )
				{
					var didDrink = wolf.DidDrink(water);
					if(didDrink == WaterState.NotDrunk)
					{
						mLog.Info("The water is still.");
						//Wait for wolf to move
						Thread.Sleep(5000);
					}
					else if(didDrink == WaterState.SeenButNotDrunk)
					{
						mLog.Info("A wolf approached, but didn't drink");
						//Wait for wolf to move
						Thread.Sleep(5000);
					}
					else
					{
						mLog.Info("The water was drunk");
						water.CoordinateX = rnd.Next(0, 30);
						water.CoordinateY = rnd.Next(0, 30);
						water.Volume = rnd.Next(1, 20);

						//5 seconds must pass before another body of water appears
						Thread.Sleep(5000);
						mLog.Info($"I am body of water id: {water.WaterId}, My coordinates are x: {water.CoordinateX}, y: {water.CoordinateY}, volume: {water.Volume}.");
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
