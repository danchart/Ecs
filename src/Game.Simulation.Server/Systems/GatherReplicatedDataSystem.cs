using Ecs.Core;
using Game.Networking.PacketData;
using Game.Simulation.Core;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Gathers all the replicated components for this tick. This is later converted into delta, prioritized, 
    /// per-entity changes per-player updates.
    /// </summary>
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicatedComponent, TransformComponent> ChangedTransformQuery = null;
        public EntityQueryWithChangeFilter<ReplicatedComponent, MovementComponent> ChangedMovementQuery = null;
        public EntityQueryWithChangeFilter<ReplicatedComponent, PlayerComponent> ChangedPlayerQuery = null;

        public IReplicationDataBroker ReplicationDataBroker = null;
        public IEntityGridMap EntityGridMap = null;

        public override void OnUpdate(float deltaTime)
        {
            // 1) Collect all modified world replicated components.
            // 2) Convert component to packet data.
            // 3) Save packet to the replication data broker.

            var modifiedEntityComponents = ReplicationDataBroker.BeginDataCollection();

            // TransformComponent

            foreach (int index in ChangedTransformQuery.GetIndices())
            {
                var entity = ChangedTransformQuery.GetEntity(index);
                ref readonly var transform = ref ChangedTransformQuery.GetReadonly2(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                transform.ToPacket(ref component.Transform);

                // Update the entity grid map.
                this.EntityGridMap.AddOrUpdate(entity, transform.position);
            }

            // MovementComponent

            foreach (int index in ChangedMovementQuery.GetIndices())
            {
                var entity = ChangedMovementQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                ChangedMovementQuery
                    .GetReadonly2(index)
                    .ToPacket(ref component.Movement);
            }

            // PlayerComponent

            foreach (int index in ChangedPlayerQuery.GetIndices())
            {
                var entity = ChangedPlayerQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                ChangedPlayerQuery
                    .GetReadonly2(index)
                    .ToPacket(ref component.Player);
            }

            ReplicationDataBroker.EndDataCollection();
        }
    }
}
 