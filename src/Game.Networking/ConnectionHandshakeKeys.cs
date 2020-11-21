using Common.Core;

namespace Game.Networking
{
    public struct ConnectionHandshakeKeys
    {
        public uint SequenceKey;
        public uint AcknowledgementKey;

        public static uint NewSequenceKey()
        {
            return RandomHelper.NextUInt();
        }

        public static uint NewAcknowledgementKey()
        {
            return RandomHelper.NextUInt();
        }
    }
}
