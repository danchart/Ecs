using Test.Common;
using Xunit;

namespace Game.Networking.Tests
{
    public class JitterBufferTests
    {
        [Fact]
        public void Test()
        {
            var logger = new TestLogger();
            var capacity = 4;
            var jitterBuffer = new PacketJitterBuffer(logger, capacity);

            //
            // Simple test: add packets in order.
            //

            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 1,
            }); ;
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 2,
            });
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 3,
            });

            Assert.Equal(3, jitterBuffer.Count);

            ReplicationPacket packet = default;

            Assert.False(jitterBuffer.TryRead(FrameIndex.Zero, ref packet));
            Assert.True(jitterBuffer.TryRead(FrameIndex.Zero + 1, ref packet));
            Assert.Equal(1, packet.FrameNumber);
            Assert.True(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
            Assert.Equal(2, packet.FrameNumber);
            Assert.True(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
            Assert.Equal(3, packet.FrameNumber);
            Assert.False(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));

            //
            // Test: Add packets out of order
            //

            jitterBuffer.Clear(FrameIndex.Zero);

            // Reverse order
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 3,
            }); ;
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 2,
            });
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 1,
            });

            Assert.Equal(3, jitterBuffer.Count);

            Assert.False(jitterBuffer.TryRead(FrameIndex.Zero, ref packet));
            Assert.True(jitterBuffer.TryRead(FrameIndex.Zero + 1, ref packet));
            Assert.Equal(1, packet.FrameNumber);
            Assert.True(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
            Assert.Equal(2, packet.FrameNumber);
            Assert.True(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
            Assert.Equal(3, packet.FrameNumber);
            Assert.False(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));

            //
            // Test: Add packets out of order with an interleaved read.
            //

            jitterBuffer.Clear(FrameIndex.Zero);

            // Reverse order
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 4,
            }); ;
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 3,
            });
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 1,
            });

            Assert.Equal(3, jitterBuffer.Count);

            Assert.False(jitterBuffer.TryRead(FrameIndex.Zero, ref packet));
            Assert.True(jitterBuffer.TryRead(FrameIndex.Zero + 1, ref packet));
            Assert.Equal(1, packet.FrameNumber);

            jitterBuffer.AddPacket(new ReplicationPacket
            {
                FrameNumber = 2,
            });

            Assert.True(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
            Assert.Equal(2, packet.FrameNumber);
            Assert.True(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
            Assert.Equal(3, packet.FrameNumber);
            Assert.True(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
            Assert.Equal(4, packet.FrameNumber);
            Assert.False(jitterBuffer.TryRead(new FrameIndex(packet.FrameNumber) + 1, ref packet));
        }
    }
}
