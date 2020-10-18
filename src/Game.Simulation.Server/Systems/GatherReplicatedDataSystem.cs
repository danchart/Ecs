using Ecs.Core;
using Game.Networking.PacketData;
using Game.Simulation.Core;

namespace Game.Simulation.Server
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicatedComponent, TransformComponent> _transformQuery = null;
        public EntityQueryWithChangeFilter<ReplicatedComponent, MovementComponent> _movementQuery = null;

        public IReplicationManager ReplicationManager = null;

        public override void OnUpdate(float deltaTime)
        {
            // Collects all world replicated components.

            var entityComponents = ReplicationManager.BeginDataCollection();

            // TransformComponent
            foreach (int index in _transformQuery.GetIndices())
            {
                var entity = _transformQuery.GetEntity(index);

                ref var component = ref entityComponents[entity].New();
                _transformQuery.GetReadonly2(index).ToPacket(ref component.Transform);
                component.FieldCount = TransformData.FieldCount;
            }

            // MovementComponent
            foreach (int index in _movementQuery.GetIndices())
            {
                var entity = _movementQuery.GetEntity(index);

                ref var component = ref entityComponents[entity].New();
                _movementQuery.GetReadonly2(index).ToPacket(ref component.Movement);
                component.FieldCount = MovementData.FieldCount;
            }

            ReplicationManager.EndDataCollection();
        }
    }
}
 