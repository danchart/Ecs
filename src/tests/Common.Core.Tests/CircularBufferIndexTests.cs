using Xunit;

namespace Common.Core.Tests
{
    public class CircularBufferIndexTests
    {
        [Fact]
        public void Test()
        {
            const int size = 10;

            // Trivial
            Assert.Equal(new CircularBufferIndex(0, size), new CircularBufferIndex(0, size));
            Assert.NotEqual(new CircularBufferIndex(1, size), new CircularBufferIndex(0, size));
            Assert.Equal(0, new CircularBufferIndex(0, size));

            // Simple
            Assert.Equal(1, new CircularBufferIndex(0, size) + 1);
            Assert.Equal(1, new CircularBufferIndex(2, size) - 1);
            Assert.Equal(new CircularBufferIndex(1, size), new CircularBufferIndex(0, size) + 1);
            Assert.Equal(new CircularBufferIndex(1, size), new CircularBufferIndex(2, size) - 1);

            // Wrap
            Assert.Equal(new CircularBufferIndex(0, size), new CircularBufferIndex(size - 1, size) + 1);
            Assert.Equal(new CircularBufferIndex(0, size) - 1, new CircularBufferIndex(size - 1, size));
        }
    }
}
