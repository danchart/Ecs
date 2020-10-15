using Ecs.Core;
using System;

namespace Ecs.Simulation.Server
{
    public interface IReplicationManager
    {
        void Sync(AppendOnlyList<AppendOnlyList<ReplicatedComponentData>> replicatedData);
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly PlayerConnectionManager PlayerConnectionManager;

        private readonly World World;

        public ReplicationManager(
            World world,
            PlayerConnectionManager playerConnectionManager)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            PlayerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));
        }

        public void Sync(AppendOnlyList<AppendOnlyList<ReplicatedComponentData>> replicatedData)
        {
            throw new NotImplementedException();
        }
    }
}
