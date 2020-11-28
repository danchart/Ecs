using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;
using System;

namespace Game.Simulation.Client
{
    public class ClientEntityReplicationSystem : SystemBase
    {
        public EntityQuery<ClientReplicationStateComponent> ReplicationStateQuery = null;

        public readonly PacketJitterBuffer PacketJitterBuffer = null;
        public readonly NetworkEntityMap NetworkEntityMap = null;
        public readonly World _world = null;

        public override void OnUpdate(float deltaTime)
        {
            if (ReplicationStateQuery.GetEntityCount() == 0)
            {
                // First tick.

                var entity = this._world.NewEntity();
                ref var newReplicationState = ref entity.GetComponent<ClientReplicationStateComponent>();

                newReplicationState.FrameIndex = FrameNumber.Zero;
            }

            ref var replicationState = ref ReplicationStateQuery.GetSingleton();

            ReplicationPacket packet = default;
            if (PacketJitterBuffer.TryRead(replicationState.FrameIndex, ref packet))
            {
                for (int i = 0; i < packet.EntityCount; i++)
                {
                    ref var entityData = ref packet.Entities[i];

                    if (!NetworkEntityMap.TryGet(entityData.NetworkEntity, out Entity entity))
                    {
                        entity = this._world.NewEntity();
                    }

                    for (int componentIndex = 0; componentIndex < entityData.ItemCount; componentIndex++)
                    {
                        ref var componentData = ref entityData.Components[componentIndex];

                        switch (componentData.Type)
                        {
                            case ComponentPacketData.TypeEnum.Transform:

                                {
                                    ref var transform = ref entity.GetComponent<TransformComponent>();

                                    transform.position.x = componentData.HasFields.Bit0 ? componentData.Transform.x : transform.position.x;
                                    transform.position.y = componentData.HasFields.Bit1 ? componentData.Transform.y : transform.position.y;
                                    transform.rotation = componentData.HasFields.Bit2 ? componentData.Transform.rotation : transform.rotation;
                                }

                                break;

                            case ComponentPacketData.TypeEnum.Movement:

                                {
                                    ref var movement = ref entity.GetComponent<MovementComponent>();

                                    movement.velocity.x = componentData.HasFields.Bit0 ? componentData.Movement.velocity_x : movement.velocity.x;
                                    movement.velocity.y = componentData.HasFields.Bit1 ? componentData.Movement.velocity_y : movement.velocity.y;
                                }

                                break;

                            case ComponentPacketData.TypeEnum.Player:

                                {
                                    ref var player = ref entity.GetComponent<PlayerComponent>();

                                    player.Id = componentData.HasFields.Bit0 ? componentData.Player.Id : player.Id;
                                }

                                break;

                            default:

                                throw new Exception($"Unknown ComponentPacketData.TypeEnum: type={componentData.Type}");
                        }
                    }
                }
            }

            replicationState.FrameIndex += 1;
        }
    }
}
