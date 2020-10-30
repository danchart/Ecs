using System;
using System.Text.Json;

namespace Database.FileSystem
{
    public sealed class DatabaseFileSystemConfig
    {
        public TimeSpan LockTimeout;

        public JsonSerializerOptions JsonSerializerOptions;

        public static readonly DatabaseFileSystemConfig Default = new DatabaseFileSystemConfig
        {
            LockTimeout = TimeSpan.FromSeconds(1),

            JsonSerializerOptions = default
        };
    }
}
