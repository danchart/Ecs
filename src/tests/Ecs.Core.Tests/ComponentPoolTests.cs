using Xunit;

namespace Ecs.Core.Tests
{
    public class ComponentPoolTests
    {
        [Fact]
        public void CloneTo()
        {
            var srcPool = new ComponentPool<SampleStructs.Foo>(2);
            IComponentPool dstPool = new ComponentPool<SampleStructs.Foo>(2); // Less than component count to test array resize.

            var idx1 = srcPool.New();
            var idx2 = srcPool.New();
            var idx3 = srcPool.New();

            srcPool.Items[idx1].Item.x = 1;
            srcPool.Items[idx1].Version = Version.Zero.GetNext();
            srcPool.Items[idx2].Item.x = 2;
            srcPool.Items[idx3].Item.x = 3;

            srcPool.CopyTo(dstPool);

            // Modify src

            srcPool.Items[idx1].Item.x = 11;
            srcPool.Items[idx1].Version = srcPool.Items[idx1].Version.GetNext();

            var typedDstPool = (ComponentPool<SampleStructs.Foo>)dstPool;

            Assert.Equal(11, srcPool.Items[idx1].Item.x);
            Assert.Equal(1, typedDstPool.Items[idx1].Item.x);
            Assert.Equal(3, typedDstPool.Items[idx3].Item.x);

            Assert.Equal(Version.Zero.GetNext().GetNext(), srcPool.Items[idx1].Version);
            Assert.Equal(Version.Zero.GetNext(), typedDstPool.Items[idx1].Version);
        }
    }
}
