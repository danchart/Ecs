﻿using Ecs.Core;
using Game.Networking.PacketData;
using Game.Simulation.Core;
using Simulation.Core;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Gathers all the replicated components for this tick. This is later converted into delta, prioritized, 
    /// per-entity changes per-player updates.
    /// </summary>
    public class ServerEntityReplicationSystem : SystemBase
    {
        public EntityQuery<ReplicatedComponent, TransformComponent>.Exclude<IsDisabledComponent> TransformQuery = null;
        public EntityQuery<ReplicatedComponent, MovementComponent>.Exclude<IsDisabledComponent> MovementQuery = null;
        public EntityQuery<ReplicatedComponent, PlayerComponent>.Exclude<IsDisabledComponent> PlayerQuery = null;

        public IReplicationDataBroker ReplicationDataBroker = null;
        public IEntityGridMap EntityGridMap = null;

        public override void OnCreate()
        {
            TransformQuery.AddEntityRemovedListener(new OnEntityRemovedCallback(RemoveEntityFromGridMap));
        }

        public override void OnUpdate(float deltaTime)
        {
            // 1) Collect all modified world replicated components.
            // 2) Convert component to packet data.
            // 3) Save packet to the replication data broker.

            var modifiedEntityComponents = ReplicationDataBroker.BeginDataCollection();

            // TransformComponent

            foreach (int index in TransformQuery.GetChangedEnumerator<TransformComponent>(this.LastSystemVersion))
            {
                var entity = TransformQuery.GetEntity(index);
                ref readonly var transform = ref TransformQuery.Get2Readonly(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                transform.ToPacket(ref component.Transform);

                // Update the entity grid map.
                this.EntityGridMap.AddOrUpdate(entity, transform.position);
            }

            // MovementComponent

            foreach (int index in MovementQuery.GetChangedEnumerator<MovementComponent>(this.LastSystemVersion))
            {
                var entity = MovementQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                MovementQuery
                    .Get2Readonly(index)
                    .ToPacket(ref component.Movement);
            }

            // PlayerComponent

            foreach (int index in PlayerQuery.GetChangedEnumerator<PlayerComponent>(this.LastSystemVersion))
            {
                var entity = PlayerQuery.GetEntity(index);

                ref var component = ref modifiedEntityComponents[entity].New();
                PlayerQuery
                    .Get2Readonly(index)
                    .ToPacket(ref component.Player);
            }

            ReplicationDataBroker.EndDataCollection();
        }

        public void RemoveEntityFromGridMap(in Entity entity)
        {
            this.EntityGridMap.Remove(entity);
        }
    }
}
 