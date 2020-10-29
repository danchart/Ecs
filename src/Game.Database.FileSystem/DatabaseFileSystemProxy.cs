using Common.Core;
using Game.Database.Core;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Game.Database.FileSystem
{
    public class DatabaseFileSystemProxy<TRecord> : IDatabaseFileSystemProxy<TRecord>
        where TRecord : struct
    {
        private readonly IFileSystem _fileSystem;
        private readonly IClock _clock;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DatabaseFileSystemProxy(
            IFileSystem fileSystem, 
            IClock clock,
            JsonSerializerOptions jsonSerializerOptions)
        {
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this._clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this._jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public bool Exists(string path)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ReadAsync(string path, ref RecordEnvelope<TRecord> record)
        {
            if (!this._fileSystem.Exists(path))
            {
                return false;
            }

            var contents = this._fileSystem.Read(path);

            record = JsonSerializer.Deserialize<RecordEnvelope<TRecord>>(
                contents, 
                this._jsonSerializerOptions);

            return true;
        }

        public async Task<bool> WriteAsync(string path, RecordEnvelope<TRecord> record)
        {
            var sentinelPath = FileSystemPaths.GetSentinelPath(path);


            var hasLock = await FileHelper.WaitForFileAsync(
                sentinelPath,
                TimeSpan.FromSeconds(1),
                this._clock);

            if (!hasLock)
            {
                return false;
            }

            File

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
