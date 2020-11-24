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

            // Roll-over case
            Assert.False(GetFrameIndexValue(1).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 10));
            Assert.False(GetFrameIndexValue(0).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 10));
            Assert.True(GetFrameIndexValue(1).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 20));
            Assert.True(GetFrameIndexValue(0).IsInRange(GetFrameIndexValue(ushort.MaxValue - 10), 11));
        }

        private static FrameIndex GetFrameIndexValue(ushort value)
        {
            FrameIndex index = FrameIndex.Zero;

            for (int i = 0; i < value; i++)
            {
                index = index.GetNext();
            }

            return index;
        }
    }
}
