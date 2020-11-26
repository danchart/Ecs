using Common.Core;
using Game.Server.Contracts;
using Game.Simulation.Core;
using System;
using System.Text.Json;
using System.Threading;

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
            var runner = new SimulationRunner(logger, gameClient, clientConfig.Simulation.FixedTick);

            runner.Start();

            logger.Info("Game client running...");

            System.Console.ReadKey(intercept: true);

            runner.Stop();
            gameClient.Stop();
        }

        private class SimulationRunner
        {
            private readonly float _fixedTick;

            private readonly GameClient _client;
            private readonly ILogger _logger;

            private AutoResetEvent _waitHandle;

            private readonly object _updateLock = new object();

            private bool _isStopped;

            public SimulationRunner(ILogger logger, GameClient client, float fixedTick)
            {
                this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
                this._client = client ?? throw new ArgumentNullException(nameof(client));
                this._fixedTick = fixedTick;
            }

            public void Start()
            {
                this._waitHandle = new AutoResetEvent(initialState: false);

                int tickMillieconds = (int)(1000 * this._fixedTick);

                _isStopped = false;

                using (var stateTimer = new Timer(
                    callback: Update,
                    state: this._waitHandle,
                    dueTime: 0,
                    period: tickMillieconds))
                {
                    _logger.Info($"World started: period={tickMillieconds}ms");

                    this._waitHandle.WaitOne();
                }
            }

            public void Stop()
            {
                this._isStopped = true;
            }

            private void Update(object obj)
            {
                // This callback can be invoked concurrently from the timer, so we must lock. Because we are simply 
                // running the simulation on the next frame & tick we can simply lock and let the next callback handle 
                // the next simulation tick.

                lock (_updateLock)
                {
                    var waitHandle = (AutoResetEvent)obj;

                    // Run simulation tick
                    this._client.FixedUpdate(
                        this._fixedTick,
                        new InputComponent
                        {
                        });

                    if (this._isStopped)
                    {
                        waitHandle.Set();
                    }
                }
            }
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
