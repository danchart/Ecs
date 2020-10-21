using Game.Simulation.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Server
{
    public class GameServer
    {
        private List<SpawnedWorld> SpawnedWorlds = new List<SpawnedWorld>();

        private PlayerConnectionManager _playerConnectionManager;

        private readonly ILogger _logger;
        private readonly IServerConfig _serverConfig;

        private ushort _nextWorldId;

        public GameServer(
            IServerConfig serverConfig,
            ILogger logger)
        {
            this._serverConfig = serverConfig  ?? throw new ArgumentNullException(nameof(serverConfig));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._playerConnectionManager = new PlayerConnectionManager(this._serverConfig.ReplicationConfig, this._serverConfig.PlayerConnectionConfig);
            this._nextWorldId = 0;
        }

        public bool IsRunning()
        {
            foreach (var spawnedWorld in this.SpawnedWorlds)
            {
                if (spawnedWorld.Thread.ThreadState != ThreadState.Stopped)
                {
                    return true;
                }
            }

            return false;
        }

        public void StopAll()
        {
            foreach (var spawnedWorld in this.SpawnedWorlds)
            {
                spawnedWorld.World.Stop();
            }
        }

        public uint SpawnWorld()
        {
            var world = new GameWorld(
                NextWorldId(), 
                this._logger,
                this._serverConfig,
                this._playerConnectionManager);
            var thread = new Thread(world.Run);
            thread.Start();

            this.SpawnedWorlds.Add(
                new SpawnedWorld
                {
                    Thread = thread,
                    World = world
                });

            this._logger.Info($"Spawning world: id={world.Id:x8}, threadId={thread.ManagedThreadId}");

            return world.Id;
        }

        private ushort NextWorldId()
        {
            if (this.SpawnedWorlds.Count == ushort.MaxValue)
            {
                var message = $"Tried to spawn more than maximum allowed number of worlds. Count={this.SpawnedWorlds.Count}";

                _logger.Error(message);

                throw new InvalidOperationException(message);
            }

            while (
                this.SpawnedWorlds
                .Where(
                    x => x.World.Id == this._nextWorldId)
                .Any())
            {
                this._nextWorldId++;
            }

            return this._nextWorldId++;
        }

        private struct SpawnedWorld
        {
            public GameWorld World;
            public Thread Thread;
        }
    }
}
