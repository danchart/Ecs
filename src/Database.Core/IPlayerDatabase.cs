using Common.Core;
using System.Threading.Tasks;

namespace Database.Core
{
    public interface IPlayerDatabase
    {
        ValueTask<RecordEnvelopeRef<PlayerRecord>> GetRecordAsync(PlayerId id);
        ValueTask<bool> SaveRecordAsync(RecordEnvelopeRef<PlayerRecord> recordRef);
    }
}
