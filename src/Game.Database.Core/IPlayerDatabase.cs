using Common.Core;

namespace Game.Database.Core
{
    public interface IPlayerDatabase
    {
        bool GetRecord(PlayerId id, ref RecordEnvelope<PlayerRecord> record);
        bool SaveRecord(in RecordEnvelope<PlayerRecord> record);
    }
}
