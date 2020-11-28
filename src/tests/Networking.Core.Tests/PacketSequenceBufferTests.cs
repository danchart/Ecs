using Xunit;

namespace Networking.Core.Tests
{
    public class PacketSequenceBufferTests
    {
        [Fact]
        public void Tests()
        {
            var buffer = new PacketSequenceBuffer(4);

            Assert.False(buffer.HasPacket(100));

            ref var packetData = ref buffer.Insert(100);
            packetData.IsAcked = true;

            Assert.True(buffer.HasPacket(100));
        }
    }
}
