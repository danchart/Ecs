using Common.Core;
using Game.Networking;
using System;

namespace Game.Server
{
    public class SimulationController
    {
        private readonly GameWorlds _worlds;
        private readonly PlayerConnectionManager _playerConnections;

        private readonly ILogger _logger;

        public SimulationController(
            ILogger logger,
            PlayerConnectionManager playerConnections,
            GameWorlds worlds)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._playerConnections = playerConnections ?? throw new ArgumentNullException(nameof(playerConnections));
            this._worlds = worlds ?? throw new ArgumentNullException(nameof(worlds));
        }

        public bool Process(PlayerId playerId, in ReplicationPacket simulationPacket)
        {
            for (int i = 0; i < simulationPacket.EntityCount; i++)
            {
                simulationPacket.Entities[i].
            }

            return true;
        }

    }
}
