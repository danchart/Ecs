using Xunit;

namespace Networking.Core.Tests
{
    public class PacketSequenceBufferTests
    {
        [Fact]
        public void Tests()
        {
            var buffer = new PacketSequenceBuffer(10);

            Assert.False(buffer.HasPacket(100));

            ref var packetData = ref buffer.Insert(100);
            packetData.IsAcked = true;

            Assert.True(buffer.HasPacket(100));

            packetData = ref buffer.Insert(101);

            // Should still have this packet since we're still adding in the same band.
            Assert.True(buffer.HasPacket(100));
            Assert.True(buffer.HasPacket(101));

            // Insert in the next band but one slot past
            packetData = ref buffer.Insert(111);

            Assert.False(buffer.HasPacket(100));
            Assert.False(buffer.HasPacket(110));
            Assert.True(buffer.HasPacket(111));

            packetData = ref buffer.Insert(110);
            Assert.True(buffer.HasPacket(110));

            packetData = ref buffer.Insert(111);
        }
    }
}
