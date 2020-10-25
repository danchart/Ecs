using Ecs.Core;
using Game.Networking;
using Game.Simulation.Server;

namespace Game.Server
{
    public interface IServerConfig
    {
        ReplicationConfig Replication { get; }
        PlayerConnectionConfig PlayerConnection { get; }
        EcsConfig Ecs { get; }
        TransportConfig Transport { get;}
        WorldConfig World { get;  }
    }

    public sealed class DefaultServerConfig : IServerConfig
    {
        public ReplicationConfig Replication => ReplicationConfig.Default;

        public PlayerConnectionConfig PlayerConnection => PlayerConnectionConfig.Default;

        public EcsConfig Ecs => EcsConfig.Default;

        public TransportConfig Transport => TransportConfig.Default;

        public WorldConfig World => WorldConfig.Default;
    }

    public sealed class TransportConfig
    {
        public int MaxPacketSize;

        public UdpPacketTransportConfig UdpPacket;

        public static readonly TransportConfig Default = new TransportConfig
        {
            MaxPacketSize = 512,
            UdpPacket = UdpPacketTransportConfig.Default,
        };
    }

    public sealed class WorldConfig
    {
        public int WorldsCapacity;

        public static readonly WorldConfig Default = new WorldConfig
        {
            WorldsCapacity = 8
        };
    }
}
