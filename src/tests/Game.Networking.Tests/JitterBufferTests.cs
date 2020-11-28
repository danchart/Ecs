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
                Sequence = 1,
            }); ;
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 2,
            });
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 3,
            });

            Assert.Equal(3, jitterBuffer.Count);

            ReplicationPacket packet = default;

            Assert.False(jitterBuffer.TryRead(FrameNumber.Zero, ref packet));
            Assert.True(jitterBuffer.TryRead(FrameNumber.Zero + 1, ref packet));
            Assert.Equal(1, packet.Sequence);
            Assert.True(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
            Assert.Equal(2, packet.Sequence);
            Assert.True(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
            Assert.Equal(3, packet.Sequence);
            Assert.False(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));

            //
            // Test: Add packets out of order
            //

            jitterBuffer.Clear(FrameNumber.Zero);

            // Reverse order
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 3,
            }); ;
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 2,
            });
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 1,
            });

            Assert.Equal(3, jitterBuffer.Count);

            Assert.False(jitterBuffer.TryRead(FrameNumber.Zero, ref packet));
            Assert.True(jitterBuffer.TryRead(FrameNumber.Zero + 1, ref packet));
            Assert.Equal(1, packet.Sequence);
            Assert.True(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
            Assert.Equal(2, packet.Sequence);
            Assert.True(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
            Assert.Equal(3, packet.Sequence);
            Assert.False(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));

            //
            // Test: Add packets out of order with an interleaved read.
            //

            jitterBuffer.Clear(FrameNumber.Zero);

            // Reverse order
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 4,
            }); ;
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 3,
            });
            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 1,
            });

            Assert.Equal(3, jitterBuffer.Count);

            Assert.False(jitterBuffer.TryRead(FrameNumber.Zero, ref packet));
            Assert.True(jitterBuffer.TryRead(FrameNumber.Zero + 1, ref packet));
            Assert.Equal(1, packet.Sequence);

            jitterBuffer.AddPacket(new ReplicationPacket
            {
                Sequence = 2,
            });

            Assert.True(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
            Assert.Equal(2, packet.Sequence);
            Assert.True(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
            Assert.Equal(3, packet.Sequence);
            Assert.True(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
            Assert.Equal(4, packet.Sequence);
            Assert.False(jitterBuffer.TryRead(new FrameNumber(packet.Sequence) + 1, ref packet));
        }
    }
}
