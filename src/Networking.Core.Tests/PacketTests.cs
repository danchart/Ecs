using System.IO;
using Xunit;

namespace Networking.Core.Tests
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

            Assert.Equal(1, resultPacket.EntityCount);
            Assert.Equal(1, resultPacket.EntityData[0].ItemCount);
            Assert.Equal(5.5f, resultPacket.EntityData[0].Items[0].Transform.x);
            Assert.Equal(7.7f, resultPacket.EntityData[0].Items[0].Transform.y);
            Assert.Equal(9.9f, resultPacket.EntityData[0].Items[0].Transform.rotation);
        }
    }
}
