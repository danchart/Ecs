using Ecs.Core;
using System;
using System.Collections.Generic;

namespace Ecs.Simulation
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicationTagComponent, TransformComponent> _transformQuery = null;

        public override void OnUpdate(float deltaTime)
        {
            // Gen 0 heap
            var replicationData = new Dictionary<Entity, AppendOnlyList<ComponentData>>();

            foreach (int index in _transformQuery.GetIndices())
            {
                var entity = _transformQuery.GetEntity(index);

                if (!replicationData.ContainsKey(entity))
                {
                    replicationData[entity] = new AppendOnlyList<ComponentData>(8);
                }

                ref var data = ref _transformQuery.Get2(index);

                replicationData[entity].Add(
                    new ComponentData
                    {
                        TypeIndex = ComponentType<TransformComponent>.Index,
                        Index = 
                    })

                
            }

            
        }

        internal class ComponentData
        {
            public int TypeIndex;
            public int Index;
        }
    }
}
 