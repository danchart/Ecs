using Xunit;

namespace Common.Core.Tests
{
    public class BitFieldTests
    {
        // Original test
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

            bitfield.Toggle(0);
            Assert.False(bitfield.Bit0);
            bitfield.Toggle(0);
            Assert.True(bitfield.Bit0);
        }
    }
}
