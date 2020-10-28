using Common.Core;
using Game.Database.Core;
using System;
using System.Text.Json;

namespace Game.Database.FileSystem
{
    public class FileSystemPlayerDatabase : IPlayerDatabase
    {
        private readonly IDatabaseFileSystemProxy<PlayerRecord> _dbFileSystemProxy;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public FileSystemPlayerDatabase(IDatabaseFileSystemProxy<PlayerRecord> dbFileSystemProxy)
        {
            this._dbFileSystemProxy = dbFileSystemProxy ?? throw new ArgumentNullException(nameof(dbFileSystemProxy));
        }

        public bool GetRecord(PlayerId id, ref RecordEnvelope<PlayerRecord> record)
        {
            var path = FileSystemPaths.GetPlayerPath(id);

            if (!this._dbFileSystemProxy.Exists(path))
            {
                return false;
            }

            var contents = this._dbFileSystemProxy.Read(path);

            return true;
        }

        public bool SaveRecord(in RecordEnvelope<PlayerRecord> record)
        {
            var path = FileSystemPaths.GetPlayerPath(record.Record.Id);

            RecordEnvelope<PlayerRecord> record

            GetRecord(PlayerId id, ref RecordEnvelope < PlayerRecord > record)


            this._fileSystem.Write(
                path, 
                JsonSerializer.Serialize(
                    record, 
                    this._jsonSerializerOptions));

            return true;
        }
    }
}
