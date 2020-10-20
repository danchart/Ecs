﻿using Ecs.Core;
using Game.Networking;
using Xunit;

namespace Game.Simulation.Server.Tests
{
    public class PlayerConnectionsTests
    {
        [Fact]
        public void Test()
        {
            var connections = new PlayerConnections(ReplicationConfig.Default, new PlayerConnectionConfig
            {
                Capacity = new PlayerConnectionConfig.CapacityConfig
                {
                    InitialConnectionsCapacity = 1
                }
            });

            Assert.Equal(0, connections.Count);

            var encryptionKey = new byte[] { 0x1 };

            var entity1 = new Entity();
            var entity2 = new Entity();
            var entity3 = new Entity();

            connections.Add(new PlayerId(0), entity1, encryptionKey);
            connections.Add(new PlayerId(1), entity2, encryptionKey);
            connections.Add(new PlayerId(2), entity3, encryptionKey);

            Assert.Equal(3, connections.Count);

            ref readonly var connection1 = ref connections[new PlayerId(0)];
            ref readonly var connection2 = ref connections[new PlayerId(1)];
            ref readonly var connection3 = ref connections[new PlayerId(2)];

            Assert.Equal(new PlayerId(0), connection1.PlayerId);
            Assert.Equal(entity1, connection1.Entity);

            connections.Remove(new PlayerId(1));

            Assert.Equal(2, connections.Count);

            var entity4 = new Entity();

            connections.Add(new PlayerId(3), entity4, encryptionKey);

            Assert.Equal(2, connections.Count);

            ref readonly var connection4 = ref connections[new PlayerId(3)];

            Assert.Equal(new PlayerId(3), connection4.PlayerId);
            Assert.Equal(entity4, connection4.Entity);

        }
    }
}