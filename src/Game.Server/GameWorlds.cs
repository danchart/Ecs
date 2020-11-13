using Common.Core;
using Game.Networking;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Game.Server
{
    public sealed class GameWorlds
    {
        private Dictionary<WorldInstanceId, GameWorldThread> _worldInstances;

        private int _nextInstanceId;

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

            this._worldInstances = new Dictionary<WorldInstanceId, GameWorldThread>(capacity);
            this._nextInstanceId = 1;
        }

        public GameWorld Get(WorldType type)
        {
            foreach (var pair in this._worldInstances)
            {
                if (pair.Value.World.WorldType == type)
                {
                    return pair.Value.World;
                }
            }

            return null;
        }

        public GameWorld Get(WorldInstanceId id) => this._worldInstances[id].World;

        public WorldInstanceId Spawn(IGameWorldFactory factory)
        {
            var worldInstanceId = new WorldInstanceId(this._nextInstanceId++);
            var world = factory.CreateInstance(worldInstanceId);
            var thread = new Thread(world.Run);

            this._worldInstances[worldInstanceId] =
                new GameWorldThread
                {
                    World = world,
                    Thread = thread,
                };

            thread.Start();

            this._logger.Info($"Spawned world instance: id={world.Id}, threadId={thread.ManagedThreadId}");

            return world.Id;
        }

        public bool Kill(WorldInstanceId id)
        {
            foreach (var pair in this._worldInstances)
            {
                pair.Value.World.Stop();
                this._worldInstances.Remove(id);

                return true;
            }

            return false;
        }

        public bool IsRunning()
        {
            foreach (var pair in this._worldInstances)
            {
                if (pair.Value.Thread.ThreadState != ThreadState.Stopped)
                {
                    return true;
                }
            }

            return false;
        }

        public void StopAll()
        {
            foreach (var pair in this._worldInstances)
            {
                pair.Value.World.Stop();
            }
        }

        public struct GameWorldThread
        {
            public GameWorld World;
            public Thread Thread;
        }
    }
}
