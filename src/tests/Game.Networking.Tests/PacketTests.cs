using Common.Core;
using Ecs.Core;
using Game.Networking.PacketData;
using System.IO;
using Xunit;

namespace Game.Networking.Tests
{
    public class PacketTests
    {
        [Fact]
        public void PacketReadWrite()
        {
            var packet = new ReplicationPacket
            {
                EntityCount = 1,
                Entities = new EntityPacketData[]
                {
                    new EntityPacketData
                    {
                        NetworkEntity = new NetworkEntity(31, 0),
                        ItemCount = 1,
                        Components = new ComponentPacketData[]
                        {
                            new ComponentPacketData
                            {
                                HasFields = new BitField
                                {
                                    Bit0 = true,
                                    Bit1 = true,
                                    Bit2 = true
                                },
                                Transform = new TransformData
                                {
                                    x = 5.5f,
                                    y = 7.7f,
                                    rotation = 9.9f
                                }
                            }
                        }
                    }
                }
            };

            var resultPacket = new ReplicationPacket();

            using (var writeStream = new MemoryStream())
            {
                packet.Serialize(writeStream, measureOnly: false);

                using (var readStream = new MemoryStream(writeStream.GetBuffer()))
                {
                    resultPacket.Deserialize(readStream);
                }
            }

            Assert.Equal(packet.EntityCount, resultPacket.EntityCount);
            Assert.Equal(packet.Entities[0].NetworkEntity, resultPacket.Entities[0].NetworkEntity);
            Assert.Equal(packet.Entities[0].ItemCount, resultPacket.Entities[0].ItemCount);
            Assert.Equal(packet.Entities[0].Components[0].Transform.x, resultPacket.Entities[0].Components[0].Transform.x);
            Assert.Equal(packet.Entities[0].Components[0].Transform.y, resultPacket.Entities[0].Components[0].Transform.y);
            Assert.Equal(packet.Entities[0].Components[0].Transform.rotation, resultPacket.Entities[0].Components[0].Transform.rotation);
        }
    }
}
