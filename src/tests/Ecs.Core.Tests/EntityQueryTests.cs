using System;
using Xunit;

namespace Ecs.Core.Tests
{
    public class EntityQueryTests
    {
        /// <summary>
        /// Validate entity query with change filtering.
        /// </summary>
        [Fact]
        public void EntityQueryWithChangeFilter()
        {
            var systems = new Systems(Helpers.NewWorld());
            var system0 = new ChangeFilterSystem<SampleStructs.FooData>();
            var system1 = new ChangeFilterSystem<SampleStructs.FooData>();
            systems
                .Add(system0)
                .Add(system1)
                .Init();

            var entity1 = systems.World.NewEntity();
            entity1.GetComponent<SampleStructs.FooData>();
            var entity2 = systems.World.NewEntity();
            entity2.GetComponent<SampleStructs.FooData>();
            var entity3 = systems.World.NewEntity();
            entity3.GetComponent<SampleStructs.FooData>();

            systems.Run(1);

            // All components dirty
            Assert.Equal(3, system0.LastFilteredEntityCount);

            entity1.GetComponent<SampleStructs.FooData>();

            systems.Run(1);

            // 1 component dirty
            Assert.Equal(1, system0.LastFilteredEntityCount);

            systems.Run(1);

            // 0 components dirty
            Assert.Equal(0, system0.LastFilteredEntityCount);

            entity1.GetReadOnlyComponent<SampleStructs.FooData>();
            entity2.GetComponent<SampleStructs.FooData>();

            systems.Run(1);

            // 1 components dirty
            Assert.Equal(1, system0.LastFilteredEntityCount);

            // Reset
            systems.Run(1);

            // 0 components dirty
            Assert.Equal(0, system0.LastFilteredEntityCount);

            // Modify component state during Run()
            system0.OnUpdateAction = () =>
            {
                ref var component = ref entity1.GetComponent<SampleStructs.FooData>();
                component.x = 17;
                component.text = "bye";
            };

            systems.Run(1);
            // Component change deferred from system0
            Assert.Equal(0, system0.LastFilteredEntityCount);
            // system1 ran after system0 will detect change
            Assert.Equal(1, system1.LastFilteredEntityCount);

            system0.OnUpdateAction = null;

            systems.Run(1);

            Assert.Equal(0, system0.LastFilteredEntityCount);
            Assert.Equal(3, system0.LastTotalEntityCount);

            ref readonly var c1 = ref entity1.GetReadOnlyComponent<SampleStructs.FooData>();

            Assert.Equal(17, c1.x);
            Assert.Equal("bye", c1.text);
        }

        internal class ChangeFilterSystem<T> : SystemBase where T : struct
        {
            public EntityQueryWithChangeFilter<T> QueryWithChangeFilter = null;
            public EntityQuery<T> Query = null;

            public int LastFilteredEntityCount = 0;
            public int LastTotalEntityCount = 0;

            public Action OnUpdateAction = null;

            public override void OnUpdate(float deltaTime)
            {
                LastFilteredEntityCount = 0;

                foreach (var entity in QueryWithChangeFilter)
                {
                    LastFilteredEntityCount++;
                }

                LastTotalEntityCount = 0;

                foreach (var entity in Query)
                {
                    OnUpdateAction?.Invoke();

                    LastTotalEntityCount++;
                }
            }
        }
    }
}
