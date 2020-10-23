using Ecs.Core;
using Ecs.Core.Collections;
using Xunit;

namespace Game.Simulation.Server.Tests
{
    public class PlayerReplicationDataPoolTests
    {
        [Fact]
        public void Tests()
        {
            var config = ReplicationConfig.Default;
            var pool = new PlayerReplicationDataPool(config, capacity: 2);
            var world = new World(EcsConfig.Default);

            var entity1 = world.NewEntity();
            var entity2 = world.NewEntity();
            var entityChangedComponents = new EntityMapList<ReplicatedComponentData>(entityCapacity: 1, listCapacity: 1);

            var idx1 = pool.New();
            Assert.Equal(1, pool.Count);

            var item1 = pool.GetItem(idx1);

            Assert.Equal(0, item1.Count);

            {
                ref var e1c1 = ref entityChangedComponents[entity1].New();
                e1c1.ComponentId = Core.ComponentId.Transform;
                e1c1.Transform.x = 1;
                e1c1.Transform.y = 2;
            }

            item1.AddEntityChanges(
                entity1,
                entityChangedComponents[entity1],
                1.0f,
                1.0f);

            Assert.Equal(1, item1.Count);

            var idx2 = pool.New();
            Assert.Equal(2, pool.Count);

            var item2 = pool.GetItem(idx2);

            Assert.Equal(0, item2.Count);

            {
                ref var e2c1 = ref entityChangedComponents[entity2].New();
                e2c1.ComponentId = Core.ComponentId.Transform;
                e2c1.Transform.x = 3;
                e2c1.Transform.y = 4;
            }

            item2.AddEntityChanges(
                entity2,
                entityChangedComponents[entity2],
                1.0f,
                1.0f);

            Assert.Equal(1, item2.Count);

            pool.Free(idx1);

            Assert.Equal(1, pool.Count);

            pool.Free(idx2);
            Assert.Equal(0, pool.Count);
        }
    }
}
