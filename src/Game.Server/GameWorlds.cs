using Common.Core;
using Game.Networking;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Game.Server
{
    public sealed class GameWorlds
    {
        private int _nextInstanceId;

        private readonly List<GameWorldThread> _worldInstances;

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

            this._worldInstances = new List<GameWorldThread>(capacity);
            this._nextInstanceId = 1;
        }

        public IEnumerable<GameWorld> GetWorlds()
        {
            foreach (var instance in this._worldInstances)
            {
                yield return instance.World;
            }
        }

        public GameWorld Get(WorldType type)
        {
            foreach (var instance in this._worldInstances)
            {
                if (instance.World.WorldType == type)
                {
                    return instance.World;
                }
            }

            return null;
        }

        public GameWorld Get(WorldInstanceId id)
        {
            foreach (var instance in this._worldInstances)
            {
                if (instance.World.InstanceId == id)
                {
                    return instance.World;
                }
            }

            return null;
        }

        public GameWorld Spawn(IGameWorldFactory factory)
        {
            var worldInstanceId = new WorldInstanceId(this._nextInstanceId++);
            var world = factory.CreateInstance(worldInstanceId);
            var thread = new Thread(world.Run);

            this._worldInstances.Add(
                new GameWorldThread
                {
                    World = world,
                    Thread = thread,
                });

            thread.Start();

            this._logger.Info($"Spawned world instance: id={world.InstanceId}, threadId={thread.ManagedThreadId}");

            return world;
        }

        public bool Kill(WorldInstanceId id)
        {
            foreach (var instance in this._worldInstances)
            {
                instance.World.Stop();
                this._worldInstances.Remove(instance);

                return true;
            }

            return false;
        }

        public bool IsRunning()
        {
            foreach (var instance in this._worldInstances)
            {
                if (instance.Thread.ThreadState != ThreadState.Stopped)
                {
                    return true;
                }
            }

            return false;
        }

        public void StopAll()
        {
            foreach (var instance in this._worldInstances)
            {
                instance.World.Stop();
            }
        }

        public void KillAll()
        {
            StopAll();

            this._worldInstances.Clear();
        }

        public List<GameWorldThread>.Enumerator GetEnumerator()
        {
            return this._worldInstances.GetEnumerator();
        }

        public struct GameWorldThread
        {
            public GameWorld World;
            public Thread Thread;
        }
    }
}
