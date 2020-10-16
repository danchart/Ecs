using Ecs.Core.Collections;
using Xunit;

namespace Ecs.Core.Tests.Collections
{
    public class EntityMapListTests
    {
        [Fact]
        public void Test()
        {
            var w = new World(EcsConfig.Default);
            var mapList = new EntityMapList<MyData>(entityCapacity: 1, listPoolCapacity: 1, listCapacity: 1);

            var entities = new Entity[1000];

            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = w.NewEntity();
            }

            // Add every even entity

            for (int i = 0; i < entities.Length; i += 2)
            {
                mapList[entities[i]].Add(new MyData
                {
                    value = i
                });

                mapList[entities[i]].Add(new MyData
                {
                    value = -i
                });
            }

            // Validate even entities

            foreach (var kv in mapList)
            {
                Assert.Equal(2, kv.Value.Count);
                Assert.Equal(kv.Key.Id, kv.Value.Items[0].value);
                Assert.Equal(kv.Key.Id, -kv.Value.Items[1].value);
            }

            // Reset map - should keep the list pool.

            mapList.Clear();

            Assert.Equal(0, mapList.Count());

            // Add every odd entity

            for (int i = 1; i < entities.Length; i += 2)
            {
                mapList[entities[i]].Add(new MyData
                {
                    value = i
                });

                mapList[entities[i]].Add(new MyData
                {
                    value = -i
                });
            }

            // Validate odd entities

            foreach (var kv in mapList)
            {
                Assert.Equal(2, kv.Value.Count);
                Assert.Equal(kv.Key.Id, kv.Value.Items[0].value);
                Assert.Equal(kv.Key.Id, -kv.Value.Items[1].value);
            }
        }

        internal struct MyData
        {
            public int value;
        }
    }
}
