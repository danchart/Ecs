using Ecs.Core;
using System;
using System.Collections.Generic;

namespace Ecs.Simulation.Server
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicationTagComponent, TransformComponent> _transformQuery = null;
        public EntityQueryWithChangeFilter<ReplicationTagComponent, MovementComponent> _movementQuery = null;

        public IReplicationManager ReplicationManager = null;

        public override void OnUpdate(float deltaTime)
        {
            // Gen 0 heap
            var entityToIndex = new Dictionary<Entity, int>();

            var replicatedData = new ReplicatedEntityData(
                entityCapacity: ReplicationManager.Config.InitialReplicatedEntityCapacity, 
                componentCapacity: ReplicationManager.Config.InitialReplicatedComponentCapacity);

            // TransformComponent
            Gather(
                _transformQuery,
                (query, index) =>
                    new ReplicatedComponentData
                    {
                        Transform = query.Ref2(index)
                    },
                entityToIndex,
                ref replicatedData);

            // MovementComponent
            Gather(
                _movementQuery,
                (query, index) =>
                    new ReplicatedComponentData
                    {
                        Movement = query.Ref2(index)
                    }
                ,
                entityToIndex,
                ref replicatedData);

            ReplicationManager.Sync(replicatedData);
        }

        private static void Gather<T>(
            EntityQueryWithChangeFilter<ReplicationTagComponent, T> query, 
            Func<EntityQueryWithChangeFilter<ReplicationTagComponent, T>, int, ReplicatedComponentData> createComponentDataFunc,
            Dictionary<Entity, int> entityToIndex,
            ref ReplicatedEntityData replicatedEntityData)
            where T : unmanaged
        {
            foreach (int index in query.GetIndices())
            {
                var entity = query.GetEntity(index);

                if (!entityToIndex.ContainsKey(entity))
                {
                    replicationData.Add(new AppendOnlyList<ReplicatedComponentData>(8));

                    entityToIndex[entity] = replicationData.Count - 1;
                }

                replicationData
                    .Items[entityToIndex[entity]]
                    .Add(
                        createComponentDataFunc(
                            query, 
                            index));
            }
        }
    }
}
 