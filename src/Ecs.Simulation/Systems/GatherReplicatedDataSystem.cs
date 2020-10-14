using Ecs.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ecs.Simulation
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicationTagComponent, TransformComponent> _transformQuery = null;
        public EntityQueryWithChangeFilter<ReplicationTagComponent, MovementComponent> _movementQuery = null;

        public override void OnUpdate(float deltaTime)
        {
            // Gen 0 heap
            var replicationData = new Dictionary<Entity, AppendOnlyList<ComponentData>>();

            // TransformComponent

            foreach (int index in _transformQuery.GetIndices())
            {
                var entity = _transformQuery.GetEntity(index);

                if (!replicationData.ContainsKey(entity))
                {
                    replicationData[entity] = new AppendOnlyList<ComponentData>(8);
                }

                replicationData[entity].Add(
                    new ComponentData
                    {
                        Transform = _transformQuery.Ref2(index)
                    });
            }

            // MovementComponent

            foreach (int index in _movementQuery.GetIndices())
            {
                var entity = _movementQuery.GetEntity(index);

                if (!replicationData.ContainsKey(entity))
                {
                    replicationData[entity] = new AppendOnlyList<ComponentData>(8);
                }

                replicationData[entity].Add(
                    new ComponentData
                    {
                        Movement = _movementQuery.Ref2(index)
                    });
            }

        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        internal struct ComponentData
        {
            [FieldOffset(0)]
            public int ComponentId;

            [FieldOffset(2)]
            public ComponentRef<TransformComponent> Transform;
            [FieldOffset(2)]
            public ComponentRef<MovementComponent> Movement;
        }
    }
}
 