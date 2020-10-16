using System.Collections.Generic;

namespace Game.Simulation.Server
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
