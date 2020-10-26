using Common.Core;
using Game.Networking;
using System;
using System.Threading;

namespace Game.Server
{
    public sealed class GameWorlds
    {
        private SpawnedWorld[] SpawnedWorlds;
        private ushort _worldCount;
        private ushort[] _freeWorldIds;
        private ushort _freeWorldCount;

        private readonly ServerChannelManager _channelManager;

        private readonly ILogger _logger;
        private readonly IServerConfig _serverConfig;

        public GameWorlds(
            ILogger logger, 
            IServerConfig serverConfig, 
            ServerChannelManager channelManager,
            int capacity)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._serverConfig = serverConfig ?? throw new ArgumentNullException(nameof(serverConfig));
            this.SpawnedWorlds = new SpawnedWorld[capacity];

            this._channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));

            this._freeWorldIds = new ushort[capacity];
            this._worldCount = 0;
            this._freeWorldCount = 0;
        }

        public WorldId Spawn()
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
                this._channelManager);
            var thread = new Thread(world.Run);
            thread.Start();

            this.SpawnedWorlds[worldId].World = world;
            this.SpawnedWorlds[worldId].Thread = thread;

            this._logger.Info($"Spawning world: id={world.Id:x8}, threadId={thread.ManagedThreadId}");

            return world.Id;
        }

        public bool IsRunning()
        {
            foreach (var gameWorld in this)
            {
                if (gameWorld.Thread.ThreadState != ThreadState.Stopped)
                {
                    return true;
                }
            }

            return false;
        }

        public void StopAll()
        {
            foreach (var gameWorld in this)
            {
                gameWorld.World.Stop();
            }
        }


        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            private GameWorlds _gameWorlds;
            private int _current;
            private int _freeIndex;

            public Enumerator(GameWorlds gameWorlds)
            {
                this._gameWorlds = gameWorlds;
                this._current = -1;
                this._freeIndex = 0;
            }

            public ref SpawnedWorld Current
            {
                get => ref this._gameWorlds.SpawnedWorlds[this._current];
            }

            public bool MoveNext()
            {
                this._current++;

                while (
                    this._current < this._gameWorlds._worldCount &&
                    this._freeIndex < this._gameWorlds._freeWorldCount &&
                    this._current == this._gameWorlds._freeWorldIds[this._freeIndex])
                {
                    this._freeIndex++;
                    this._current++;
                }

                return _current < this._gameWorlds._worldCount;
            }
        }

        public struct SpawnedWorld
        {
            public GameWorld World;
            public Thread Thread;
        }
    }
}
