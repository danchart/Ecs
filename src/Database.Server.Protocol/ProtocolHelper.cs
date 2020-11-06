using System.Net;

namespace Database.Server.Protocol
{
    public static class ProtocolHelper
    {
        public static IPEndPoint GetServerEndPointFromIpAddress(IPAddress ipAddress) => new IPEndPoint(ipAddress, ProtocolConstants.ServerPort);
    }
}
