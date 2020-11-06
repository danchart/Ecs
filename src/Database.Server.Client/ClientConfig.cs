using System.Net;

namespace Database.Server.Client
{
    public class ClientConfig
    {
        public IPAddress HostIPAddress;

        public int TcpMessageQueueCapacity;

        public static readonly ClientConfig Default = new ClientConfig
        {
            HostIPAddress = IPAddress.Parse("127.0.0.1"),

            TcpMessageQueueCapacity = 128,
        };
    }
}
