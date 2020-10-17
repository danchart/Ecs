using Xunit;

namespace Common.Core.Tests
{
    public class BitFieldTests
    {
        [Fact]
        public void Test()
        {
            var bitfield = new BitField();

            bitfield.Set(0);
            bitfield.Set(1);

            Assert.True(bitfield.Bit0);
            Assert.True(bitfield.IsSet(0));
            Assert.True(bitfield.Bit1);
            Assert.True(bitfield.IsSet(1));
            Assert.False(bitfield.Bit2);
            Assert.False(bitfield.IsSet(2));

            bitfield.Unset(0);
            bitfield.Unset(1);
            bitfield.Unset(2); // no op

            Assert.False(bitfield.Bit0);
            Assert.False(bitfield.Bit1);
            Assert.False(bitfield.Bit2);

            bitfield.SetAll(2);
            Assert.True(bitfield.Bit0);
            Assert.True(bitfield.IsSet(0));
            Assert.True(bitfield.Bit1);
            Assert.True(bitfield.IsSet(1));
            Assert.False(bitfield.Bit2);
            Assert.False(bitfield.IsSet(2));
        }
    }
}
