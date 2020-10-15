using System.Collections.Generic;

namespace Ecs.Simulation.Server
{
    public interface IPlayerConnectionManager
    {
        Dictionary<int, PlayerConnection> Connections { get; }
    }

    public class PlayerConnectionManager : IPlayerConnectionManager
    {
        public Dictionary<int, PlayerConnection> Connections { get; } = new Dictionary<int, PlayerConnection>();
    }
}
