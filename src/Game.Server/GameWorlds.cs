using Common.Core;
using Game.Networking;
using System;
using System.Threading;

namespace Game.Server
{
    public sealed class GameWorlds
    {
        private GameWorldThread[] _spawnedWorlds;
        private ushort _worldCount;
        private ushort[] _freeWorldIds;
        private ushort _freeWorldCount;

        private readonly ServerChannelOutgoing _channelManager;

        private readonly ILogger _logger;
        private readonly IServerConfig _serverConfig;

        public GameWorlds(
            ILogger logger, 
            IServerConfig serverConfig, 
            ServerChannelOutgoing channelManager,
            int capacity)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._serverConfig = serverConfig ?? throw new ArgumentNullException(nameof(serverConfig));
            this._spawnedWorlds = new GameWorldThread[capacity];

            this._channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));

            this._freeWorldIds = new ushort[capacity];
            this._worldCount = 0;
            this._freeWorldCount = 0;
        }

        public GameWorld Get(WorldId id)
        {
            return this._spawnedWorlds[id].World;
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
                if (this._worldCount == this._spawnedWorlds.Length)
                {
                    Array.Resize(ref this._spawnedWorlds, 2 * this._worldCount);
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

            this._spawnedWorlds[worldId].World = world;
            this._spawnedWorlds[worldId].Thread = thread;

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

            public ref GameWorldThread Current
            {
                get => ref this._gameWorlds._spawnedWorlds[this._current];
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

        public struct GameWorldThread
        {
            public GameWorld World;
            public Thread Thread;
        }
    }
}
