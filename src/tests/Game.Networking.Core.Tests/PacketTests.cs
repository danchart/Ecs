using System.IO;
using Xunit;

namespace Game.Networking.Core.Tests
{
    public class PacketTests
    {
        [Fact]
        public void PacketReadWrite()
        {
            var packet = new SimulationPacket
            {
                EntityCount = 1,
                EntityData = new EntityPacketData[]
                {
                    new EntityPacketData
                    {
                        EntityId = 31,
                        ItemCount = 1,
                        Items = new PacketDataItem[]
                        {
                            new PacketDataItem
                            {
                                HasFields = new BitField
                                {
                                    Bit0 = true,
                                    Bit1 = true,
                                    Bit2 = true
                                },
                                Transform = new PacketDataItem.TransformData
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

            var resultPacket = new SimulationPacket();

            using (var writeStream = new MemoryStream())
            {
                packet.Serialize(writeStream);

                using (var readStream = new MemoryStream(writeStream.GetBuffer()))
                {
                    resultPacket.Deserialize(readStream);
                }
            }

            Assert.Equal(packet.EntityCount, resultPacket.EntityCount);
            Assert.Equal(packet.EntityData[0].EntityId, resultPacket.EntityData[0].EntityId);
            Assert.Equal(packet.EntityData[0].ItemCount, resultPacket.EntityData[0].ItemCount);
            Assert.Equal(packet.EntityData[0].Items[0].Transform.x, resultPacket.EntityData[0].Items[0].Transform.x);
            Assert.Equal(packet.EntityData[0].Items[0].Transform.y, resultPacket.EntityData[0].Items[0].Transform.y);
            Assert.Equal(packet.EntityData[0].Items[0].Transform.rotation, resultPacket.EntityData[0].Items[0].Transform.rotation);
        }
    }
}
