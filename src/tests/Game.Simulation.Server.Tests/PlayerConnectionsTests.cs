using Common.Core;
using Game.Networking;
using System.Net;
using Test.Common;
using Xunit;

namespace Game.Simulation.Server.Tests
{
    public class PlayerConnectionsTests
    {
        [Fact]
        public void Test()
        {
            var logger = new TestLogger();
            var connections = new PlayerConnectionManager(
                logger,
                new PlayerConnectionConfig
            {
                Capacity = new PlayerConnectionConfig.CapacityConfig
                {
                    InitialConnectionsCapacity = 1
                }
            });

            Assert.Equal(0, connections.Count);

            var worldId = new WorldInstanceId(0);
            var encryptionKey = new byte[] { 0x1 };
            var endPoint = new IPEndPoint(0, 0);

            connections.Add(worldId, new PlayerId(0), encryptionKey, endPoint);
            connections.Add(worldId, new PlayerId(1), encryptionKey, endPoint);
            connections.Add(worldId, new PlayerId(2), encryptionKey, endPoint);

            Assert.Equal(3, connections.Count);

            ref readonly var connection1 = ref connections[new PlayerId(0)];
            ref readonly var connection2 = ref connections[new PlayerId(1)];
            ref readonly var connection3 = ref connections[new PlayerId(2)];

            Assert.Equal(new PlayerId(0), connection1.PlayerId);

            connections.Remove(new PlayerId(1));

            Assert.Equal(2, connections.Count);
            Assert.False(connections.HasPlayer(new PlayerId(1)));

            connections.Add(worldId, new PlayerId(3), encryptionKey, endPoint);

            Assert.Equal(3, connections.Count);

            ref readonly var connection4 = ref connections[new PlayerId(3)];

            Assert.Equal(new PlayerId(3), connection4.PlayerId);
        }
    }
}
