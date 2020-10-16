using Ecs.Core;
using Ecs.Core.Collections;
using Game.Networking.PacketData;
using Game.Simulation.Core;
using System;

namespace Game.Simulation.Server
{
    public class GatherReplicatedDataSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<ReplicationTagComponent, TransformComponent> _transformQuery = null;
        public EntityQueryWithChangeFilter<ReplicationTagComponent, MovementComponent> _movementQuery = null;

        public IReplicationManager ReplicationManager = null;

        public override void OnUpdate(float deltaTime)
        {
            // Collect all the replicated entities

            var entityComponents = ReplicationManager.EntityComponents;

            // TransformComponent
            foreach (int index in _transformQuery.GetIndices())
            {
                var entity = _transformQuery.GetEntity(index);

                entityComponents[entity].New();
                entityComponents[entity].Current.Transform = _transformQuery.GetReadonly2(index).ToPacket();
            }

            // TransformComponent
            //Gather(
            //    _transformQuery,
            //    (query, index) =>
            //        new ReplicatedComponentData
            //        {
            //            //Transform = query.GetReadonly2(index).ToPacket()
            //        },
            //    entityComponents);

            //// MovementComponent
            //Gather(
            //    _movementQuery,
            //    (query, index) =>
            //        new ReplicatedComponentData
            //        {
            //            //Movement = query.Ref2(index)
            //        }
            //    ,
            //    entityComponents);

            ReplicationManager.Sync();
        }

        private static void Gather<T>(
            EntityQueryWithChangeFilter<ReplicationTagComponent, T> query, 
            // TODO: This Func<> probably prevents an important inlining opportunity.
            //      Using it for now as it saves a lot of typing and code duplication.
            Func<EntityQueryWithChangeFilter<ReplicationTagComponent, T>, int, ReplicatedComponentData> newComponentDataFunc,
            MOTHBALL_EntityMapList<ReplicatedComponentData> replicatedEntityData)
            where T : unmanaged
        {
            foreach (int index in query.GetIndices())
            {
                var entity = query.GetEntity(index);

                replicatedEntityData[entity].Add(
                    newComponentDataFunc(
                        query, 
                        index));
            }
        }
    }
}
 