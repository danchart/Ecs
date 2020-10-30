using Common.Core;
using Database.Core;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Database.FileSystem
{
    public class DatabaseFileSystemProxy<TRecord> : IDatabaseFileSystemProxy<TRecord>
        where TRecord : struct
    {
        private readonly IFileSystem _fileSystem;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly RecordEnvelopePool<TRecord> _recordEnvelopePool;
        private readonly DatabaseFileSystemConfig _config;

        public DatabaseFileSystemProxy(
            IFileSystem fileSystem, 
            IClock clock,
            ILogger logger,
            RecordEnvelopePool<TRecord> recordEnvelopePool,
            DatabaseFileSystemConfig config)
        {
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this._clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._recordEnvelopePool = recordEnvelopePool ?? throw new ArgumentNullException(nameof(recordEnvelopePool));
            this._config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public bool Exists(string path)
        {
            return this._fileSystem.Exists(path);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async ValueTask<RecordEnvelopeRef<TRecord>> ReadAsync(string path)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (!this._fileSystem.Exists(path))
            {
                throw new InvalidOperationException($"File not found: path={path}");
            }

            var contents = this._fileSystem.Read(path);

            var index = this._recordEnvelopePool.New();
            var recordRef = this._recordEnvelopePool.Ref(index);

            recordRef.Unref() = JsonSerializer.Deserialize<RecordEnvelope<TRecord>>(
                contents, 
                this._config.JsonSerializerOptions);

            return recordRef;
        }

        public async ValueTask<bool> WriteAsync(string path, RecordEnvelopeRef<TRecord> recordRef)
        {
            var sentinelPath = FileSystemPaths.GetSentinelPath(path);

            var isSuccess = await FileHelper.AcquireFileLockAsync(
                sentinelPath,
                this._clock,
                this._config.LockTimeout);

            if (!isSuccess)
            {
                this._logger.Warning($"Failed to acquire lock for record write: path={path}");

                return false;
            }

            try
            {
                var currentRecordRef = await ReadAsync(path);

                if (currentRecordRef.Unref().ETag != recordRef.Unref().ETag)
                {
                    return false;
                }

                this._fileSystem.Write(
                    path,
                    JsonSerializer.Serialize(
                        recordRef.Unref(),
                        this._config.JsonSerializerOptions));

                return true;
            }
            finally
            {
                FileHelper.ReleaseFileLock(sentinelPath);
            }
        }
    }
}
