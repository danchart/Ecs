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

                entityComponents[entity].New();
                _transformQuery.GetReadonly2(index).ToPacket(ref entityComponents[entity].Current.Transform);
            }

            // MovementComponent
            foreach (int index in _movementQuery.GetIndices())
            {
                var entity = _movementQuery.GetEntity(index);

                entityComponents[entity].New();
                _movementQuery.GetReadonly2(index).ToPacket(ref entityComponents[entity].Current.Movement);
            }

            ReplicationManager.EndDataCollection();
        }
    }
}
 