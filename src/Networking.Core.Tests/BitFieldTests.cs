using Xunit;

namespace Networking.Core.Tests
{
    public class BitFieldTests
    {
        [Fact]
        public void Set()
        {
            var bitField = new BitField();

            bitField.Set(0);
            bitField.Set(3);

            Assert.True(bitField.IsSet(0));
            Assert.True(bitField.IsSet(3));
            Assert.False(bitField.IsSet(1));

            bitField.Unset(3);

            Assert.False(bitField.IsSet(3));
        }
    }
}
