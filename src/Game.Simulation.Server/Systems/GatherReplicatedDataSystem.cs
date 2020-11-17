using Ecs.Core;
using Game.Networking.PacketData;
using Game.Simulation.Core;
using System;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Gathers all the replicated components for this tick. This is later converted into delta, prioritized, 
    /// per-entity changes per-player updates.
    /// </summary>
    public class GatherReplicatedDataSystem : SystemBase
    {
        public ChangedEntityQuery<ReplicatedComponent, TransformComponent> ChangedTransformQuery = null;
        public ChangedEntityQuery<ReplicatedComponent, MovementComponent> ChangedMovementQuery = null;
        public ChangedEntityQuery<ReplicatedComponent, PlayerComponent> ChangedPlayerQuery = null;

        public IReplicationDataBroker ReplicationDataBroker = null;
        public IEntityGridMap EntityGridMap = null;

        public override void OnCreate()
        {
            ChangedTransformQuery.AddListener(new EntityGridMapListener(this.EntityGridMap));
        }

        public override void OnUpdate(float deltaTime)
        {
            // 1) Collect all modified world replicated components.
            // 2) Convert component to packet data.
            // 3) Save packet to the replication data broker.

            var modifiedEntityComponents = ReplicationDataBroker.BeginDataCollection();

            // TransformComponent

            foreach (int index in ChangedTransformQuery.GetIndices2(this.LastSystemVersion))
            {
                var entity = ChangedTransformQuery.GetEntity(index);
                ref readonly var transform = ref ChangedTransformQuery.GetReadonly2(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                transform.ToPacket(ref component.Transform);

                // Update the entity grid map.
                this.EntityGridMap.AddOrUpdate(entity, transform.position);
            }

            // MovementComponent

            foreach (int index in ChangedMovementQuery.GetIndices2(this.LastSystemVersion))
            {
                var entity = ChangedMovementQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                ChangedMovementQuery
                    .GetReadonly2(index)
                    .ToPacket(ref component.Movement);
            }

            // PlayerComponent

            foreach (int index in ChangedPlayerQuery.GetIndices2(this.LastSystemVersion))
            {
                var entity = ChangedPlayerQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                ChangedPlayerQuery
                    .GetReadonly2(index)
                    .ToPacket(ref component.Player);
            }

            ReplicationDataBroker.EndDataCollection();
        }

        private class EntityGridMapListener : IEntityQueryListener
        {
            private readonly IEntityGridMap _entityGridMap;

            public EntityGridMapListener(IEntityGridMap entityGridMap)
            {
                this._entityGridMap = entityGridMap ?? throw new ArgumentNullException(nameof(entityGridMap));
            }

            public void OnEntityAdded(in Entity entity)
            {
                // Do nothing, will add to grid map in the system.
            }

            public void OnEntityRemoved(in Entity entity)
            {
                this._entityGridMap.Remove(entity);
            }
        }
    }
}
 