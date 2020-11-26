using Common.Core;
using Game.Networking;
using Game.Simulation.Server;
using System;

namespace Game.Server
{
    public class SimulationServerController
    {
        private readonly GameWorlds _worlds;
        private readonly PlayerConnectionManager _playerConnections;

        private readonly ILogger _logger;

        public SimulationServerController(
            ILogger logger,
            PlayerConnectionManager playerConnections,
            GameWorlds worlds)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._playerConnections = playerConnections ?? throw new ArgumentNullException(nameof(playerConnections));
            this._worlds = worlds ?? throw new ArgumentNullException(nameof(worlds));
        }

        public bool Process(PlayerId playerId, in ClientInputPacket inputPacket)
        {
            if (!this._playerConnections.HasPlayer(playerId))
            {
                this._logger.VerboseError($"Received input packet for non-existent player: id={playerId}");

                return false;
            }

            ref var connection = ref this._playerConnections.Get(playerId);

            var world = this._worlds.Get(connection.WorldInstanceId);

            // TODO: Execute/queue inputs against connection.Entity in simulation

            //world.

            return true;
        }

    }
}
