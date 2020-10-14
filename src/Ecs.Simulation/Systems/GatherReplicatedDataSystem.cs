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
            Gather(
                _transformQuery,
                (query, index) =>
                    new ComponentData
                    {
                        Transform = query.Ref2(index)
                    },
                replicationData);

            // MovementComponent

            Gather(
                _movementQuery,
                (query, index) =>
                    new ComponentData
                    {
                        Movement = query.Ref2(index)
                    }
                ,
                replicationData);


            //foreach (int index in _transformQuery.GetIndices())
            //{
            //    var entity = _transformQuery.GetEntity(index);

            //    if (!replicationData.ContainsKey(entity))
            //    {
            //        replicationData[entity] = new AppendOnlyList<ComponentData>(8);
            //    }

            //    replicationData[entity].Add(
            //        new ComponentData
            //        {
            //            Transform = _transformQuery.Ref2(index)
            //        });
            //}

            // MovementComponent

            //foreach (int index in _movementQuery.GetIndices())
            //{
            //    var entity = _movementQuery.GetEntity(index);

            //    if (!replicationData.ContainsKey(entity))
            //    {
            //        replicationData[entity] = new AppendOnlyList<ComponentData>(8);
            //    }

            //    replicationData[entity].Add(
            //        new ComponentData
            //        {
            //            Movement = _movementQuery.Ref2(index)
            //        });
            //}

        }

        private static void Gather<T>(
            EntityQueryWithChangeFilter<ReplicationTagComponent, T> query, 
            Func<EntityQueryWithChangeFilter<ReplicationTagComponent, T>, int, ComponentData> createComponentDataFunc,
            Dictionary<Entity, AppendOnlyList<ComponentData>> replicationData)
            where T : unmanaged
        {
            foreach (int index in query.GetIndices())
            {
                var entity = query.GetEntity(index);

                if (!replicationData.ContainsKey(entity))
                {
                    replicationData[entity] = new AppendOnlyList<ComponentData>(8);
                }

                replicationData[entity].Add(createComponentDataFunc(query, index));
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
 