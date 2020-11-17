using System;
using Xunit;

namespace Ecs.Core.Tests
{
    public class EntityQueryTests
    {
        [Fact]
        public void EntityQuery()
        {
            // No system Add's
            var systems = new Systems(Helpers.NewWorld());

            systems.Create();

            var querySingleInclude =
                systems.World
                .GetEntityQuery<EntityQuery<SampleStructs.Foo>>();

            int listenerCount = 0;

            querySingleInclude.AddListener(new TestListener
            {
                OnAdd = (entity) => listenerCount++,
                OnRemove = (entity) => listenerCount--
            });

            // Create test entities
            var entity1 = systems.World.NewEntity();
            var entity2 = systems.World.NewEntity();

            Assert.Equal(0, querySingleInclude.GetEntityCount());
            Assert.Equal(querySingleInclude.GetEntityCount(), listenerCount);

            // Add inclusion #1
            entity1.GetComponent<SampleStructs.Foo>();

            Assert.Equal(1, querySingleInclude.GetEntityCount());
            Assert.Equal(querySingleInclude.GetEntityCount(), listenerCount);

            // Add inclusion #2
            entity2.GetComponent<SampleStructs.Foo>();

            Assert.Equal(2, querySingleInclude.GetEntityCount());
            Assert.Equal(querySingleInclude.GetEntityCount(), listenerCount);

            // Remove inclusion #1 
            entity1.RemoveComponent<SampleStructs.Foo>();

            Assert.Equal(1, querySingleInclude.GetEntityCount());
            Assert.Equal(querySingleInclude.GetEntityCount(), listenerCount);

            // Remove inclusion #2 - should place entity in query results.
            entity2.RemoveComponent<SampleStructs.Foo>();

            Assert.Equal(0, querySingleInclude.GetEntityCount());
            Assert.Equal(querySingleInclude.GetEntityCount(), listenerCount);
        }

        [Fact]
        public void EntityQueryMultiType()
        {
            // No system Add's
            var systems = 
                new Systems(Helpers.NewWorld());

            systems.Create();

            var query =
                systems.World
                .GetEntityQuery<EntityQuery<SampleStructs.Foo, SampleStructs.Bar>>();

            var entity3 = systems.World.NewEntity();

            entity3.GetComponent<SampleStructs.Foo>();

            Assert.Equal(0, query.GetEntityCount());

            entity3.GetComponent<SampleStructs.Bar>();

            Assert.Equal(1, query.GetEntityCount());
        }

        [Fact]
        public void EntityQuery_IncludeExclude()
        {
            var systems = new Systems(Helpers.NewWorld());
            
            systems
                // No system Add's
                .Create();

            var queryWithExclude = 
                systems.World
                .GetEntityQuery<EntityQuery<SampleStructs.Foo>.Exclude<SampleStructs.Bar, SampleStructs.Baz>>();

            // Create 2 entities, one with exclusions
            var entity1 = systems.World.NewEntity();
            entity1.GetComponent<SampleStructs.Foo>();
            var entity2 = systems.World.NewEntity();
            // Include
            entity2.GetComponent<SampleStructs.Foo>();
            // Exclude #1
            entity2.GetComponent<SampleStructs.Bar>();
            // Exclude #2
            entity2.GetComponent<SampleStructs.Baz>();

            Assert.Equal(1, queryWithExclude.GetEntityCount());

            // Remove exclusion #1
            entity2.RemoveComponent<SampleStructs.Bar>();

            Assert.Equal(1, queryWithExclude.GetEntityCount());

            // Remove exclusion #2 - should place entity in query results.
            entity2.RemoveComponent<SampleStructs.Baz>();

            Assert.Equal(2, queryWithExclude.GetEntityCount());
        }

        /// <summary>
        /// Validate entity query with change filtering.
        /// </summary>
        [Fact]
        public void EntityQueryWithChangeFilter()
        {
            var systems = new Systems(Helpers.NewWorld());
            var system0 = new ChangeFilterSystem<SampleStructs.Foo>();
            var system1 = new ChangeFilterSystem<SampleStructs.Foo>();
            systems
                .Add(system0)
                .Add(system1)
                .Create();

            var entity1 = systems.World.NewEntity();
            entity1.GetComponent<SampleStructs.Foo>();
            var entity2 = systems.World.NewEntity();
            entity2.GetComponent<SampleStructs.Foo>();
            var entity3 = systems.World.NewEntity();
            entity3.GetComponent<SampleStructs.Foo>();

            GC.Collect();
            var cc1 = GC.CollectionCount(0);

            systems.Run(1);

            // All components dirty
            Assert.Equal(3, system0.LastFilteredEntityCount);

            entity1.GetComponent<SampleStructs.Foo>();

            systems.Run(1);

            // 1 component dirty
            Assert.Equal(1, system0.LastFilteredEntityCount);

            systems.Run(1);

            // 0 components dirty
            Assert.Equal(0, system0.LastFilteredEntityCount);

            entity1.GetReadOnlyComponent<SampleStructs.Foo>();
            entity2.GetComponent<SampleStructs.Foo>();

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
                ref var component = ref entity1.GetComponent<SampleStructs.Foo>();
                component.x = 17;
                component.y = 42;
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

            ref readonly var c1 = ref entity1.GetReadOnlyComponent<SampleStructs.Foo>();

            Assert.Equal(17, c1.x);
            Assert.Equal(42, c1.y);

            GC.Collect();
            var cc2 = GC.CollectionCount(0);

            int i = 0;
        }

        internal class ChangeFilterSystem<T> : SystemBase 
            where T : unmanaged
        {
            public ChangedEntityQuery<T> QueryWithChangeFilter = null;
            public EntityQuery<T> Query = null;

            public int LastFilteredEntityCount = 0;
            public int LastTotalEntityCount = 0;

            public Action OnUpdateAction = null;

            public override void OnUpdate(float deltaTime)
            {
                LastFilteredEntityCount = 0;

                foreach (var entity in QueryWithChangeFilter.GetEntities(this.LastSystemVersion))
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

        internal class TestListener : IEntityQueryListener
        {
            public Action<Entity> OnAdd;
            public Action<Entity> OnRemove;

            public void OnEntityAdded(in Entity entity)
            {
                OnAdd?.Invoke(entity);
            }

            public void OnEntityRemoved(in Entity entity)
            {
                OnRemove?.Invoke(entity);
            }
        }
    }
}
