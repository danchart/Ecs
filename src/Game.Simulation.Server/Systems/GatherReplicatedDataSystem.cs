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
        public EntityQueryWithChangeFilter<ReplicatedComponent, TransformComponent> _transformQuery = null;
        public EntityQueryWithChangeFilter<ReplicatedComponent, MovementComponent> _movementQuery = null;
        public EntityQueryWithChangeFilter<ReplicatedComponent, PlayerComponent> _playerQuery = null;

        public IReplicationDataBroker ReplicationDataBroker = null;

        public override void OnUpdate(float deltaTime)
        {
            // 1) Collect all modified world replicated components.
            // 2) Convert component to packet data.
            // 3) Save packet to the replication data broker.

            var modifiedEntityComponents = ReplicationDataBroker.BeginDataCollection();

            // TransformComponent
            foreach (int index in _transformQuery.GetIndices())
            {
                var entity = _transformQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                _transformQuery
                    .GetReadonly2(index)
                    .ToPacket(ref component.Transform);
            }

            // MovementComponent
            foreach (int index in _movementQuery.GetIndices())
            {
                var entity = _movementQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                _movementQuery
                    .GetReadonly2(index)
                    .ToPacket(ref component.Movement);
            }

            // PlayerComponent
            foreach (int index in _playerQuery.GetIndices())
            {
                var entity = _playerQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                _playerQuery
                    .GetReadonly2(index)
                    .ToPacket(ref component.Player);
            }

            ReplicationDataBroker.EndDataCollection();
        }
    }
}
 