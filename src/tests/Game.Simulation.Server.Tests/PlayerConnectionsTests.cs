using Ecs.Core;
using Xunit;

namespace Game.Simulation.Server.Tests
{
    public class PlayerConnectionsTests
    {
        [Fact]
        public void Test()
        {
            var connections = new PlayerConnections(ReplicationConfig.Default, capacity: 1);

            Assert.Equal(0, connections.Count);

            var entity1 = new Entity();
            var entity2 = new Entity();
            var entity3 = new Entity();

            connections.Add(0, entity1);
            connections.Add(1, entity2);
            connections.Add(2, entity3);

            Assert.Equal(3, connections.Count);

            ref readonly var connection1 = ref connections[0];
            ref readonly var connection2 = ref connections[1];
            ref readonly var connection3 = ref connections[2];

            Assert.Equal(0, connection1.PlayerId);
            Assert.Equal(entity1, connection1.Entity);

            connections.Remove(1);

            Assert.Equal(2, connections.Count);

            var entity4 = new Entity();

            connections.Add(3, entity4);

            Assert.Equal(2, connections.Count);

            ref readonly var connection4 = ref connections[3];

            Assert.Equal(3, connection4.PlayerId);
            Assert.Equal(entity4, connection4.Entity);

        }
    }
}
