using Ecs.Core;
using System.Collections.Generic;

namespace Ecs.Simulation
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicationComponent, TransformComponent> _transformQuery = null;

        public override void OnUpdate(float deltaTime)
        {
            var entityToComponents = new Dictionary<Entity, Dictionary<ComponentId, string>>();
        }
    }
}
 