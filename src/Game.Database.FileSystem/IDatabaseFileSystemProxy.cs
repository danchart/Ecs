using Game.Database.Core;
using System.Threading.Tasks;

namespace Game.Database.FileSystem
{
    public interface IDatabaseFileSystemProxy<TRecord>
        where TRecord : struct
    {
        bool Exists(string path);

        ValueTask<RecordEnvelopeRef<TRecord>> ReadAsync(string path);
        ValueTask<bool> WriteAsync(string path, RecordEnvelopeRef<TRecord> recordRef);
    }
}
