using Ecs.Core;
using System;
using System.Collections.Generic;

namespace Ecs.Simulation.Server
{
    public interface IReplicationManager
    {
        void Sync(Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>> replicatedData);
    }

    public class ReplicationManager : IReplicationManager
    {
        private Dictionary<Entity, int> _clients = new Dictionary<Entity, int>();

        private readonly World World;

        public ReplicationManager(World world)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
        }

        public void Sync(Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>> replicatedData)
        {
            throw new NotImplementedException();
        }
    }
}
