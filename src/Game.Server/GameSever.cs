using Common.Core;
using Game.Networking;
using Game.Simulation.Server;
using System;
using System.Threading;

namespace Game.Server
{
    public class GameServer
    {
        private SpawnedWorld[] SpawnedWorlds;
        private ushort _worldCount;
        private ushort[] _freeWorldIds;
        private ushort _freeWorldCount;

        private PlayerConnectionManager _playerConnectionManager;

        private readonly ILogger _logger;
        private readonly IServerConfig _serverConfig;

        public GameServer(
            IServerConfig serverConfig,
            ILogger logger)
        {
            this._serverConfig = serverConfig  ?? throw new ArgumentNullException(nameof(serverConfig));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._playerConnectionManager = new PlayerConnectionManager(this._serverConfig.Replication, this._serverConfig.PlayerConnection);

            this.SpawnedWorlds = new SpawnedWorld[8];
            this._freeWorldIds = new ushort[8];
            this._worldCount = 0;
            this._freeWorldCount = 0;
        }

        public bool IsRunning()
        {
            foreach (var spawnedWorld in this)
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
            foreach (var spawnedWorld in this)
            {
                spawnedWorld.World.Stop();
            }
        }

        public WorldId SpawnWorld()
        {
            int worldId;

            if (this._freeWorldCount > 0)
            {
                worldId = this._freeWorldIds[this._freeWorldCount--];
            }
            else
            {
                if (this._worldCount == this.SpawnedWorlds.Length)
                {
                    Array.Resize(ref this.SpawnedWorlds, 2 * this._worldCount);
                }

                worldId = this._worldCount++;
            }

            var world = new GameWorld(
                new WorldId(worldId), 
                this._logger,
                this._serverConfig,
                this._playerConnectionManager);
            var thread = new Thread(world.Run);
            thread.Start();

            this.SpawnedWorlds[worldId].World = world;
            this.SpawnedWorlds[worldId].Thread = thread;

            this._logger.Info($"Spawning world: id={world.Id:x8}, threadId={thread.ManagedThreadId}");

            return world.Id;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            private GameServer _gameServer;
            private int _current;
            private int _freeIndex;

            public Enumerator(GameServer gameServer)
            {
                this._gameServer = gameServer;
                this._current = -1;
                this._freeIndex = 0;
            }

            public ref SpawnedWorld Current
            {
                get => ref this._gameServer.SpawnedWorlds[this._current];
            }

            public bool MoveNext()
            {
                this._current++;

                while (
                    this._current < this._gameServer._worldCount &&
                    this._freeIndex < this._gameServer._freeWorldCount &&
                    this._current == this._gameServer._freeWorldIds[this._freeIndex])
                {
                    this._freeIndex++;
                    this._current++;
                }

                return _current < this._gameServer._worldCount;
            }
        }

        public struct SpawnedWorld
        {
            public GameWorld World;
            public Thread Thread;
        }
    }
}
