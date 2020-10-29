using Common.Core;
using Game.Database.Core;
using System;
using System.Threading.Tasks;

namespace Game.Database.FileSystem
{
    public class FileSystemPlayerDatabase : IPlayerDatabase
    {
        private readonly IDatabaseFileSystemProxy<PlayerRecord> _dbFileSystemProxy;

        public FileSystemPlayerDatabase(IDatabaseFileSystemProxy<PlayerRecord> dbFileSystemProxy)
        {
            this._dbFileSystemProxy = dbFileSystemProxy ?? throw new ArgumentNullException(nameof(dbFileSystemProxy));
        }

        public async ValueTask<RecordEnvelopeRef<PlayerRecord>> GetRecordAsync(PlayerId id)
        {
            var path = FileSystemPaths.GetPlayerPath(id);

            return await this._dbFileSystemProxy.ReadAsync(path);
        }

        public async ValueTask<bool> SaveRecordAsync(RecordEnvelopeRef<PlayerRecord> recordRef)
        {
            var path = FileSystemPaths.GetPlayerPath(recordRef.Unref().Record.Id);

            return await this._dbFileSystemProxy.WriteAsync(path, recordRef);
        }
    }
}
