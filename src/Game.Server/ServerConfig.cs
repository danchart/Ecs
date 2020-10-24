using Ecs.Core;
using Game.Simulation.Server;

namespace Game.Server
{
    public interface IServerConfig
    {
        ReplicationConfig Replication { get; }
        PlayerConnectionConfig PlayerConnection { get; }
        EcsConfig Ecs { get; }
        TransportConfig Transport { get;}
    }

    public sealed class DefaultServerConfig : IServerConfig
    {
        public ReplicationConfig Replication => ReplicationConfig.Default;

        public PlayerConnectionConfig PlayerConnection => PlayerConnectionConfig.Default;

        public EcsConfig Ecs => EcsConfig.Default;

        public TransportConfig Transport => TransportConfig.Default;
    }

    public sealed class TransportConfig
    {
        public int MaxPacketSize;

        public static readonly TransportConfig Default = new TransportConfig
        {
            MaxPacketSize = 512
        };
    }
}
