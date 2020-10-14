using Ecs.Core;

namespace Ecs.Simulation
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        private EntityQueryWithChangeFilter<ReplicateEntityComponent, ReplicateEntityComponent> _foo;

        public override void OnUpdate(float deltaTime)
        {
            
        }
    }
}
 