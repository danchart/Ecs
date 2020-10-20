using Game.Simulation.Server;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Game.Server
{
    public class GameServer
    {
        //
        // Configurations
        //

        private readonly ReplicationConfig _replicationConfig = ReplicationConfig.Default;
        private readonly PlayerConnectionConfig _playerConnectionConfig = PlayerConnectionConfig.Default;

        private List<SpawnedWorld> SpawnedWorlds = new List<SpawnedWorld>();

        private PlayerConnectionManager _playerConnectionManager;

        private readonly ILogger _logger;

        private Random Random = new Random();

        public GameServer(ILogger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._playerConnectionManager = new PlayerConnectionManager(this._replicationConfig, this._playerConnectionConfig);
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
                GenerateWorldId(), 
                this._logger,
                ,
                this._replicationConfig,
                this._playerConnectionConfig);
            var thread = new Thread(world.Run);
            thread.Start();

            this.SpawnedWorlds.Add(new SpawnedWorld
            {
                Thread = thread,
                World = world
            });

            this._logger.Info($"Spawning world: id={world.Id:x8}, threadId={thread.ManagedThreadId}");

            return world.Id;
        }

        private uint GenerateWorldId()
        {
            while (true)
            {
                uint id = (uint)this.Random.Next(int.MaxValue);

                var isUnique = true;

                foreach (var spawnedWorld in this.SpawnedWorlds)
                {
                    if (id == spawnedWorld.World.Id)
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                {
                    return id;
                }
            }
        }

        private struct SpawnedWorld
        {
            public GameWorld World;
            public Thread Thread;
        }
    }
}
