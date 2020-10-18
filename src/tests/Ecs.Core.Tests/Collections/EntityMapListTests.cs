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
            var mapList = new EntityMapList<MyData>(entityCapacity: 1, listCapacity: 1); // capacity 1 validates array growth

            var entities = new Entity[1000];

            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = w.NewEntity();
            }

            // Add every even entity

            for (int i = 0; i < entities.Length; i += 2)
            {
                mapList[entities[i]].New();
                mapList[entities[i]].Current.value = i;
                mapList[entities[i]].New();
                mapList[entities[i]].Current.value = -i;
            }

            // Validate even entities

            foreach (var item in mapList)
            {
                Assert.Equal(2, item.Items.Count);

                // Direct access
                Assert.Equal(item.Entity.Id, item.Items[0].value);
                Assert.Equal(item.Entity.Id, -item.Items[1].value);

                // Loop
                for (int i = 0; i < item.Items.Count; i++)
                {
                    Assert.Equal(i == 0 ? item.Entity.Id : -item.Entity.Id, item.Items[i].value);
                }
            }

            // Reset map - should keep the list pool.

            mapList.Clear();

            Assert.Equal(0, mapList.Count);

            // Add every odd entity

            for (int i = 1; i < entities.Length; i += 2)
            {
                mapList[entities[i]].New();
                mapList[entities[i]].Current.value = i;
                mapList[entities[i]].New();
                mapList[entities[i]].Current.value = -i;
            }

            // Validate odd entities

            foreach (var item in mapList)
            {
                Assert.Equal(2, item.Items.Count);

                // Direct access
                Assert.Equal(item.Entity.Id, item.Items[0].value);
                Assert.Equal(item.Entity.Id, -item.Items[1].value);

                // ItemList for loop
                for (int i = 0; i < item.Items.Count; i++)
                {
                    Assert.Equal(i == 0 ? item.Entity.Id : -item.Entity.Id, item.Items[i].value);
                }

                // ItemList foreach loop
                int count = 0;
                foreach (var subItem in item.Items)
                {
                    Assert.Equal(count++ == 0 ? item.Entity.Id : -item.Entity.Id, subItem.value);
                }
            }

            // Validate versioning

            mapList.Clear();
            mapList[entities[0]].New().value = 11;
            mapList[entities[0]].New().value = 12;
            Assert.Equal(2, mapList[entities[0]].Count);
            Assert.Equal(11, mapList[entities[0]][0].value);
            Assert.Equal(12, mapList[entities[0]][1].value);
        }

        internal struct MyData
        {
            public int value;
        }
    }
}
