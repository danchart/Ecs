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
        JitterConfig Jitter { get; }
    }

    public class JitterConfig
    {
        public int Capacity;

        public static readonly JitterConfig Default = new JitterConfig
        {
            Capacity = 64,
        };
    }

    public class DefaultClientConfig : IClientConfig
    {
        public EcsConfig Ecs => EcsConfig.Default;

        public PlayerInputConfig PlayerInput => PlayerInputConfig.Default;

        public SimulationConfig Simulation => SimulationConfig.Default;

        public NetworkTransportConfig NetworkTransport => NetworkTransportConfig.Default;

        public JitterConfig Jitter => JitterConfig.Default;
    }
}
