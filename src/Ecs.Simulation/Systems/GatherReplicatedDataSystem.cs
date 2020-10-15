using Ecs.Core;
using System;

namespace Ecs.Simulation.Server
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicationTagComponent, TransformComponent> _transformQuery = null;
        public EntityQueryWithChangeFilter<ReplicationTagComponent, MovementComponent> _movementQuery = null;

        public IReplicationManager ReplicationManager = null;

        public override void OnUpdate(float deltaTime)
        {
            var entityComponents = ReplicationManager.EntityComponents;
            entityComponents.Clear();

            // TransformComponent
            Gather(
                _transformQuery,
                (query, index) =>
                    new ReplicatedComponentData
                    {
                        Transform = query.Ref2(index)
                    },
                entityComponents);

            // MovementComponent
            Gather(
                _movementQuery,
                (query, index) =>
                    new ReplicatedComponentData
                    {
                        Movement = query.Ref2(index)
                    }
                ,
                entityComponents);

            ReplicationManager.Sync();
        }

        private static void Gather<T>(
            EntityQueryWithChangeFilter<ReplicationTagComponent, T> query, 
            // TODO: This Func<> probably prevents an important inlining opportunity.
            //      Using it for now as it saves a lot of typing and code duplication.
            Func<EntityQueryWithChangeFilter<ReplicationTagComponent, T>, int, ReplicatedComponentData> newComponentDataFunc,
            ReplicatedEntities replicatedEntityData)
            where T : unmanaged
        {
            foreach (int index in query.GetIndices())
            {
                var entity = query.GetEntity(index);

                replicatedEntityData[entity]
                    .Add(
                        newComponentDataFunc(
                            query, 
                            index));
            }
        }
    }
}
 