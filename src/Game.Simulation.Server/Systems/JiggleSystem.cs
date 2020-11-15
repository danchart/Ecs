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
            foreach (ref var transform in Query.GetComponents2())
            {
                transform.position
            }
        }
    }
}
 