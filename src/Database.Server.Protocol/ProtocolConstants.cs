namespace Database.Server.Protocol
{
    public static class ProtocolConstants
    {
        public const int ServerPort = 27166;

        // 65KB minus protocol overhead buffer (256 bytes chosen arbitrarily).
        public const int MaxTcpMessageSize = (1 << 16) - 256; 
    }
}
