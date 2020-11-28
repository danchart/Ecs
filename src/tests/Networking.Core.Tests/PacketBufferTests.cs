using System.IO;
using System.Net;
using Xunit;

namespace Networking.Core.Tests
{
    public class PacketBufferTests
    {
        [Fact]
        public void Tests()
        {
            var encryptor = new XorPacketEncryptor();
            var buffer = new PacketBuffer<PacketData>(encryptor, 4);
            var data = new byte[1024];

            var testPacket = new PacketEnvelope<PacketData>
            {
                Header = new PacketEnvelopeHeader
                {
                    Sequence = 100,
                    Ack = 0,
                    AckBitField = 0
                },
                Contents = new PacketData
                {
                    a = 11,
                    b = 12,
                    c = 13
                }

            };

            Assert.False(buffer.HasPacket(100));

            int size;

            using (var stream = new MemoryStream(data))
            {
                size = testPacket.Serialize(stream, encryptor);
            }

            buffer.AddPacket(data, 0, size, new IPEndPoint(IPAddress.Any, 0));

            Assert.True(buffer.HasPacket(100));
            Assert.Equal(11, buffer.GetPacket(100).Contents.a);
            Assert.Equal(12, buffer.GetPacket(100).Contents.b);
            Assert.Equal(13, buffer.GetPacket(100).Contents.c);

            testPacket.Header.Sequence = 101;

            using (var stream = new MemoryStream(data))
            {
                size = testPacket.Serialize(stream, encryptor);
            }

            buffer.AddPacket(data, 0, size, new IPEndPoint(IPAddress.Any, 0));

            Assert.True(buffer.HasPacket(101));

            // Overwrite sequence 100 with sequence 104
            testPacket.Header.Sequence = 104;
            testPacket.Contents.a = 21;
            testPacket.Contents.b = 22;
            testPacket.Contents.c = 23;

            using (var stream = new MemoryStream(data))
            {
                size = testPacket.Serialize(stream, encryptor);
            }

            buffer.AddPacket(data, 0, size, new IPEndPoint(IPAddress.Any, 0));

            Assert.False(buffer.HasPacket(100));
            Assert.True(buffer.HasPacket(104));
            Assert.Equal(21, buffer.GetPacket(104).Contents.a);
            Assert.Equal(22, buffer.GetPacket(104).Contents.b);
            Assert.Equal(23, buffer.GetPacket(104).Contents.c);
        }

        private struct PacketData : IPacketSerialization
        {
            public int a, b, c;

            public bool Deserialize(Stream stream)
            {
                stream.PacketReadInt(out this.a);
                stream.PacketReadInt(out this.b);
                stream.PacketReadInt(out this.c);

                return true;
            }

            public int Serialize(Stream stream)
            {
                return 
                    stream.PacketWriteInt(this.a)
                    + stream.PacketWriteInt(this.b)
                    + stream.PacketWriteInt(this.c);
            }
        }
    }
}
