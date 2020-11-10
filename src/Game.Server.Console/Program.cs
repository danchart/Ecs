using Common.Core;
using Networking.Server;
using System;
using System.Net;

namespace Game.Server.Console
{
    class Program
    {
        private static readonly ILogger _logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            _logger.Info("Hello, starting the game server.");

            GameServer gameServer = new GameServer(DefaultServerConfig.Instance, _logger);

            HttpServer httpServer = new GameHttpServer(_logger, gameServer);

            httpServer.Start(new string[] { "http://localhost:8110/" });

            IGameWorldLoader worldLoader = new GameWorldLoader(new WorldType("dummy"));

            gameServer.SpawnWorld(worldLoader);
            //gameServer.SpawnWorld();

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

        private class GameHttpServer : HttpServer
        {
            private readonly GameServer _gameServer;

            public GameHttpServer(ILogger logger, GameServer gameServer) 
                : base(logger)
            {
                this._gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
            }

            protected override void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
            {
                string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
        }
    }
}
