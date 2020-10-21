using Game.Server.Tests.Helpers;
using Game.Simulation.Server;
using Xunit;

namespace Game.Server.Tests
{
    public class GameWorldTests
    {
        [Fact]
        public void Test()
        {
            var logger = new TestLogger();
            var config = new DefaultServerConfig();

            var playerConnections = new PlayerConnectionManager(config.ReplicationConfig, config.PlayerConnectionConfig);

            var gameWorld = new GameWorld(
                id: 0,
                logger: logger,
                config: config,
                playerConnections: playerConnections);
        }
    }
}
