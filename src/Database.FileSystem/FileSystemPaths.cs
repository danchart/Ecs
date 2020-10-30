using Common.Core;

namespace Database.FileSystem
{
    public static class FileSystemPaths
    {
        public static string GetPlayerPath(PlayerId id) => $"players\\{id}";

        public static string GetSentinelPath(string path) => $"{path}\\gate.db";
    }
}
