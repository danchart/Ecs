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
        public IPEndPoint HostIpEndPoint;

        public int MaxTcpPacketSize;
        public int TcpPacketReceiveQueueCapacity;

        public static readonly ServerConfig Default = new ServerConfig
        {
            HostIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27001),

            MaxTcpPacketSize = 2048,
            TcpPacketReceiveQueueCapacity = 128,
        };
    }
}
