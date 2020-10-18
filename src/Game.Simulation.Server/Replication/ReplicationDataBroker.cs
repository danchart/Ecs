using Ecs.Core;
using Ecs.Core.Collections;
using System;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Brokers replication data from the simulation to the networking system.
    /// </summary>
    public interface IReplicationDataBroker
    {
        EntityMapList<ReplicatedComponentData> BeginDataCollection();
        void EndDataCollection();
    }

    public class ReplicationDataBroker : IReplicationDataBroker
    {
        private readonly IReplicationManager _replicationManager;

        private readonly EntityMapList<ReplicatedComponentData> _entityComponents;

        public ReplicationDataBroker(
            ReplicationConfig config,
            IReplicationManager replicationManager)
        {
            this._replicationManager = replicationManager ?? throw new ArgumentNullException(nameof(replicationManager));

            this._entityComponents = new EntityMapList<ReplicatedComponentData>(
                entityCapacity: config.InitialReplicatedEntityCapacity,
                listCapacity: config.InitialReplicatedComponentCapacity);
        }

        public EntityMapList<ReplicatedComponentData> BeginDataCollection()
        {
            // Invalidate any previously collected data.
            this._entityComponents.Clear();

            return this._entityComponents;
        }

        public void EndDataCollection()
        {
            _replicationManager.Apply(_entityComponents);
        }
    }
}
