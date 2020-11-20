using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;

namespace Game.Client
{
    public interface IClientConfig
    {
        EcsConfig Ecs { get; }
        PlayerInputConfig PlayerInput { get; }
        SimulationConfig Simulation { get;  }
        NetworkTransportConfig NetworkTransport { get; } 
    }

    public class DefaultClientConfig : IClientConfig
    {
        public EcsConfig Ecs => EcsConfig.Default;

        public PlayerInputConfig PlayerInput => PlayerInputConfig.Default;

        public SimulationConfig Simulation => SimulationConfig.Default;

        public NetworkTransportConfig NetworkTransport => NetworkTransportConfig.Default;
    }
}
