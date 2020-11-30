using Xunit;

namespace Networking.Core.Tests
{
    public class PacketSequenceBufferTests
    {
        [Fact]
        public void Tests()
        {
            var buffer = new PacketSequenceBuffer(10);

            Assert.False(buffer.Contains(100));

            ref var packetData = ref buffer.Insert(100);
            //packetData.IsAcked = true;

            Assert.True(buffer.Contains(100));
            Assert.Equal(100U, buffer.Ack);
            //Assert.Equal(0U, buffer.GetAckBitfield());

            packetData = ref buffer.Insert(101);

            // Should still have this packet since we're still adding in the same band.
            Assert.True(buffer.Contains(100));
            Assert.True(buffer.Contains(101));

            //Assert.Equal(1U, buffer.GetAckBitfield());

            // Insert in the next band but one slot past
            packetData = ref buffer.Insert(111);

            Assert.False(buffer.Contains(100));
            Assert.False(buffer.Contains(110));
            Assert.True(buffer.Contains(111));

            packetData = ref buffer.Insert(110);

            Assert.True(buffer.Contains(110));
            Assert.True(buffer.Contains(111));

            //packetData = ref buffer.Insert(111);
            //Assert.Equal(3U, buffer.GetAckBitfield());
        }
    }
}
