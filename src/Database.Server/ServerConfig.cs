using System.Net;

namespace Database.Server
{
    public interface IDatabaseServerConfig
    {
        ServerConfig Server { get; }
    }

    public sealed class DefaultDatabaseServerConfig : IDatabaseServerConfig
    {
        public static readonly DefaultDatabaseServerConfig Instance = new DefaultDatabaseServerConfig();

        public ServerConfig Server => ServerConfig.Default;
    }

    public class ServerConfig
    {
        public IPAddress HostIpAddress;

        public int TcpClientCapacity;

        public int TcpMessageQueueCapacity;

        public static readonly ServerConfig Default = new ServerConfig
        {
            HostIpAddress = IPAddress.Parse("127.0.0.1"),

            TcpClientCapacity = 16,

            TcpMessageQueueCapacity = 128,
        };
    }
}
