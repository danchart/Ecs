using System.Net;

namespace Game.Server
{
    public class UdpServerConfig
    {
        /// <summary>
        /// UDP server host endpoint.
        /// </summary>
        public IPEndPoint HostIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        public static readonly UdpServerConfig Default = new UdpServerConfig();
    }
}
