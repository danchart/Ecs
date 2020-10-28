using Common.Core;

namespace Game.Database.Core
{
    public struct PlayerRecord
    {
        public PlayerId Id;
        public byte[] SessionKey;
    }
}
