using Common.Core.Numerics;
using Ecs.Core;
using Xunit;

namespace Game.Simulation.Server.Tests
{
    public class EntityGridMapTests
    {
        [Fact]
        public void Test()
        {
            IEntityGridMap entityMap = new EntityGridMap(gridSize: 10.0f);

            var world = new World(EcsConfig.Default);

            var entity1 = world.NewEntity();
            var position1 = new Vector2(0, 0);
            var entity2 = world.NewEntity();
            var position2 = new Vector2(1, 1);
            var entity3 = world.NewEntity();
            var position3 = new Vector2(100, 100);

            entityMap.AddOrUpdate(entity1, position1);
            entityMap.AddOrUpdate(entity2, position2);
            entityMap.AddOrUpdate(entity3, position3);

            entityMap.GetGridPosition(Vector2.Zero, out int row, out int column);

            var entities = entityMap.GetEntities(row, column);

            Assert.Equal(2, entities.Count);

            position1 = new Vector2(101.0f, 101.0f);

            entityMap.AddOrUpdate(entity1, position1);

            entities = entityMap.GetEntities(row, column);

            Assert.Equal(1, entities.Count);

            entityMap.GetGridPosition(position1, out row, out column);

            entities = entityMap.GetEntities(row, column);

            Assert.Equal(2, entities.Count);
            Assert.Contains(entity1, entities);
            Assert.Contains(entity3, entities);

            entityMap.Remove(entity1);
            entityMap.Remove(entity3);

            entities = entityMap.GetEntities(row, column);

            Assert.Equal(0, entities.Count);
        }
    }
}
