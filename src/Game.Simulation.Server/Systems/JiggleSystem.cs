using Ecs.Core;
using Game.Networking.PacketData;
using Game.Simulation.Core;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Gathers all the replicated components for this tick. This is later converted into delta, prioritized, 
    /// per-entity changes per-player updates.
    /// </summary>
    public class JiggleSystem : SystemBase
    {
        public EntityQuery<ReplicatedComponent, TransformComponent>.Exclude<PlayerComponent> Query = null;

        public override void OnUpdate(float deltaTime)
        {
            foreach (int index in Query.GetIndices())
            {
                var entity = ChangedTransformQuery.GetEntity(index);
                ref readonly var transform = ref ChangedTransformQuery.GetReadonly2(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                transform.ToPacket(ref component.Transform);

                // Update the entity grid map.
                this.EntityGridMap.AddOrUpdate(entity, transform.position);
            }
        }
    }
}
 