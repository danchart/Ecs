using Ecs.Core;
using Game.Networking;

namespace Game.Simulation.Client
{
    public class ClientEntityReplicationSystem : SystemBase
    {
        public EntityQuery<ClientReplicationStateComponent> ReplicationStateQuery = null;

        public PacketJitterBuffer PacketJitterBuffer;
        public EntityServerToClientMap EntityServerToClientMap;

        public override void OnUpdate(float deltaTime)
        {
            ref var replicationState = ref ReplicationStateQuery.GetSingleton();

            ReplicationPacket packet = default;
            if (PacketJitterBuffer.TryRead(replicationState.FrameIndex, ref packet))
            {
                for (int i = 0; i < packet.EntityCount; i++)
                {
                    ref var serverEntityData = ref packet.Entities[i];

                    var entity = EntityExtensions.DeserializeFromPacketData(
                        id: serverEntityData.EntityId,
                        generation: serverEntityData.EntityGeneration,
                        world: this.Wor)

                    if (EntityServerToClientMap.TryGet())
                }
            }

            replicationState.FrameIndex += 1;
        }
    }
}
