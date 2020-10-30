using Common.Core;

namespace Database.Server.Console
{
    class Program
    {
        private static readonly ILogger _logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            _logger.Info("Hello, starting the game server.");

            IDatabaseServerConfig dbServerConfig = DefaultDatabaseServerConfig.Instance;

            var dbServer = new DatabaseServer(_logger, dbServerConfig.Server);

            dbServer.Start();

            _logger.Info("Running...");

            bool isStopping = false;

            while (dbServer.IsRunning)
            {
                if (!isStopping &&
                    System.Console.KeyAvailable)
                {
                    isStopping = true;

                    // Consume key.
                    System.Console.ReadKey(intercept: true);

                    dbServer.Stop();
                }
            }

            _logger.Info("Stopped.");
            _logger.Info("Press any key to exit.");

            System.Console.ReadKey(intercept: true);
        }
    }
}
