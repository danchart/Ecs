using Common.Core;
using Networking.Server;

namespace Game.Server.Console
{
    class Program
    {
        private static readonly ILogger _logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            _logger.Info("Hello, starting the game server.");

            GameServer gameServer = new GameServer(DefaultServerConfig.Instance, _logger);

            gameServer.Start();

            HttpServer httpServer = new GameHttpServer(_logger, gameServer);

            httpServer.Start(new string[] { "http://localhost:8110/" });

            var spawnedWorld = gameServer
                .Commander
                .RunCommandAsync(
                    new SpawnWorldCommand(
                        new WorldType("eden")))
                .Result;

            _logger.Info("Running...");

            bool isStopping = false;

            while (gameServer.IsRunning())
            {
                if (!isStopping &&
                    System.Console.KeyAvailable)
                {
                    isStopping = true;

                    // Consume key.
                    System.Console.ReadKey(intercept: true);

                    gameServer.StopAll();
                }
            }

            _logger.Info("Stopped.");
            _logger.Info("Press any key to exit.");

            System.Console.ReadKey(intercept: true);
        }
    }
}
