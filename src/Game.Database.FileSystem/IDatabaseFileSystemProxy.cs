using Game.Database.Core;
using System.Threading.Tasks;

namespace Game.Database.FileSystem
{
    public interface IDatabaseFileSystemProxy<TRecord>
        where TRecord : struct
    {
        bool Exists(string path);

        Task<bool> ReadAsync(string path, ref RecordEnvelope<TRecord> record);
        Task<bool> WriteAsync(string path, RecordEnvelope<TRecord> record);
    }
}
