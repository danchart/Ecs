using Common.Core;
using Game.Server.Contracts;
using System.Text.Json;

namespace Game.Client.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            IClientConfig clientConfig = new DefaultClientConfig();
            ILogger logger = new ConsoleLogger();
            IJsonSerializer jsonSerializer = new MyJsonSerializer();

            logger.Info("Hello, press any key to start the game client console.");

            System.Console.ReadKey(intercept: true);

            var gameClient = new GameClient(logger, jsonSerializer, clientConfig);

            gameClient.Start("http://localhost:8110");

            logger.Info("Game client running...");

            System.Console.ReadKey(intercept: true);

            gameClient.Stop();
        }

        private class MyJsonSerializer : IJsonSerializer
        {
            private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
            {
                IncludeFields = true
            };

            public FailureResponseBody Deserialize_FailureResponseBody(string jsonValue)
            {
                return JsonSerializer.Deserialize<FailureResponseBody>(jsonValue, SerializerOptions);
            }

            public PostPlayerConnectResponseBody Deserialize_PostPlayerConnectResponseBody(string jsonValue)
            {
                return JsonSerializer.Deserialize<PostPlayerConnectResponseBody>(jsonValue, SerializerOptions);
            }

            public string Serialize<T>(T value)
            {
                return JsonSerializer.Serialize(value);
            }
        }
    }
}
