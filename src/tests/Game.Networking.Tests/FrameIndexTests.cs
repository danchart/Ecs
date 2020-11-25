using Xunit;

namespace Game.Networking.Tests
{
    public class FrameIndexTests
    {
        [Fact]
        public void TestIsInrange()
        {
            // Trivial case.
            Assert.False(FrameIndex.Zero.IsInRange(GetFrameIndexValue(1), 1));
            Assert.True(FrameIndex.Zero.IsInRange(FrameIndex.Zero, 0));
            Assert.True(FrameIndex.Zero.IsInRange(FrameIndex.Zero, 1));

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
            Assert.Equal(0, FrameIndex.Compare(FrameIndex.Zero, FrameIndex.Zero));

            // Simple.
            Assert.Equal(1, FrameIndex.Compare(FrameIndex.Zero, GetFrameIndexValue(1)));
            Assert.Equal(-1, FrameIndex.Compare(GetFrameIndexValue(1), FrameIndex.Zero));

            // Rollover.
            Assert.Equal(1, FrameIndex.Compare(GetFrameIndexValue(ushort.MaxValue - 1), GetFrameIndexValue(1)));
            Assert.Equal(-1, FrameIndex.Compare(GetFrameIndexValue(1), GetFrameIndexValue(ushort.MaxValue - 1)));
        }

        private static FrameIndex GetFrameIndexValue(ushort value)
        {
            return FrameIndex.Zero + value;
        }
    }
}
