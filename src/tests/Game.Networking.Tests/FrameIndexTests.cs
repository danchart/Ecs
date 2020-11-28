using Xunit;

namespace Game.Networking.Tests
{
    public class FrameIndexTests
    {
        [Fact]
        public void TestIsInrange()
        {
            // Trivial case.
            Assert.False(FrameNumber.Zero.IsInRange(GetFrameIndexValue(1), 1));
            Assert.True(FrameNumber.Zero.IsInRange(FrameNumber.Zero, 0));
            Assert.True(FrameNumber.Zero.IsInRange(FrameNumber.Zero, 1));

            // Simple non-rollover case.
            Assert.False(GetFrameIndexValue(100).IsInRange(GetFrameIndexValue(0), 20));
            Assert.True(GetFrameIndexValue(10).IsInRange(GetFrameIndexValue(0), 20));

            // Rollover case
            Assert.False(GetFrameIndexValue(1).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 10));
            Assert.False(GetFrameIndexValue(0).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 10));
            Assert.True(GetFrameIndexValue(1).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 20));
            Assert.True(GetFrameIndexValue(0).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 11));
        }

        [Fact]
        public void TestCompare()
        {
            // Trivial.
            Assert.Equal(0, FrameNumber.Compare(FrameNumber.Zero, FrameNumber.Zero));

            // Simple.
            Assert.Equal(1, FrameNumber.Compare(FrameNumber.Zero, GetFrameIndexValue(1)));
            Assert.Equal(-1, FrameNumber.Compare(GetFrameIndexValue(1), FrameNumber.Zero));

            // Rollover.
            Assert.Equal(1, FrameNumber.Compare(GetFrameIndexValue(ushort.MaxValue - 1), GetFrameIndexValue(1)));
            Assert.Equal(-1, FrameNumber.Compare(GetFrameIndexValue(1), GetFrameIndexValue(ushort.MaxValue - 1)));
        }

        private static FrameNumber GetFrameIndexValue(ushort value)
        {
            return FrameNumber.Zero + value;
        }
    }
}
