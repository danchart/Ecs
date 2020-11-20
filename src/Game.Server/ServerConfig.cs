using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;
using Game.Simulation.Server;

namespace Game.Server
{
    public interface IServerConfig
    {
        ReplicationConfig Replication { get; }
        PlayerConnectionConfig PlayerConnection { get; }
        EcsConfig Ecs { get; }
        TransportConfig Transport { get; }
        ServerConfig Server { get; }
        PlayerInputConfig PlayerInput { get;  }
        SimulationConfig Simulation { get; }
    }

    public sealed class DefaultServerConfig : IServerConfig
    {
        public static readonly DefaultServerConfig Instance = new DefaultServerConfig();

        public ReplicationConfig Replication => ReplicationConfig.Default;

        public PlayerConnectionConfig PlayerConnection => PlayerConnectionConfig.Default;

        public EcsConfig Ecs => EcsConfig.Default;

        public TransportConfig Transport => TransportConfig.Default;

        public ServerConfig Server => ServerConfig.Default;

        public PlayerInputConfig PlayerInput => PlayerInputConfig.Default;

        public SimulationConfig Simulation => SimulationConfig.Default;
    }

    public sealed class TransportConfig
    {
        public int MaxPacketSize;

        public UdpPacketTransportConfig UdpPacket;
        public IPacketEncryption PacketEncryption;

        public static readonly TransportConfig Default = new TransportConfig
        {
            MaxPacketSize = 512,
            UdpPacket = UdpPacketTransportConfig.Default,
            PacketEncryption = new XorPacketEncryption(),
        };
    }

    public sealed class ServerConfig
    {
        public int WorldsCapacity;

        public static readonly ServerConfig Default = new ServerConfig
        {
            WorldsCapacity = 8,
        };
    }
}
