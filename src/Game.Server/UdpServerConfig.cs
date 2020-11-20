using System.Net;

namespace Game.Server
{
    public class UdpServerConfig
    {
        public int ReceivePacketQueueCapacity = 256;
        public int SendPacketQueueCapacity = 128;

        /// <summary>
        /// UDP server host endpoint.
        /// </summary>
        public IPEndPoint HostIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        public static readonly UdpServerConfig Default = new UdpServerConfig();
    }
}
