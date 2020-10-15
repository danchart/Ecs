using System.Collections.Generic;

namespace Ecs.Simulation.Server
{
    public interface IPlayerConnectionManager
    {
        Dictionary<int, PlayerConnection> Connections { get; }
    }

    public class PlayerConnectionManager : IPlayerConnectionManager
    {
        private readonly Dictionary<int, PlayerConnection> _connections = new Dictionary<int, PlayerConnection>();

        public Dictionary<int, PlayerConnection> Connections => _connections;
    }
}
