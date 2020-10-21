using Ecs.Core;
using Game.Simulation.Server;

namespace Game.Server
{
    public interface IServerConfig
    {
        ReplicationConfig ReplicationConfig { get;  }
        PlayerConnectionConfig PlayerConnectionConfig { get; }
        EcsConfig EcsConfig { get; }
    }

    public class DefaultServerConfig : IServerConfig
    {
        public ReplicationConfig ReplicationConfig => ReplicationConfig.Default;

        public PlayerConnectionConfig PlayerConnectionConfig => PlayerConnectionConfig.Default;

        public EcsConfig EcsConfig => EcsConfig.Default;
    }
}
