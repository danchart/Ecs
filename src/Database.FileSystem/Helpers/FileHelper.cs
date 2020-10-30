using Common.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Database.FileSystem
{
    public static class FileHelper
    {
        public static int WaitForFileRetryDelay = 10; // in milliseconds

        public static async Task<bool> WaitForFileAsync(
            string path, 
            TimeSpan timeout,
            IClock clock,
            CancellationToken cancellationToken = default)
        {
            var timeoutAt = clock.UtcNow + timeout;

            while (File.Exists(path))
            {
                if (clock.UtcNow >= timeoutAt)
                {
                    return false;
                }

                await Task.Delay(WaitForFileRetryDelay, cancellationToken);
            }

            return true;
        }

        public static async ValueTask<bool> AcquireFileLockAsync(
            string sentinelPath,
            IClock clock,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            var timeoutAt = clock.UtcNow + timeout;

            while (timeoutAt >= clock.UtcNow)
            {
                while (File.Exists(sentinelPath))
                {
                    if (clock.UtcNow >= timeoutAt)
                    {
                        return false;
                    }

                    await Task.Delay(WaitForFileRetryDelay, cancellationToken);
                }

                try
                {
                    using (var stream = File.Open(
                        sentinelPath,
                        FileMode.CreateNew,
                        FileAccess.ReadWrite,
                        FileShare.Write))
                    {
                        stream.Close();

                        // Successfully acquired lock.
                        return true;
                    }
                }
                catch (IOException)
                {
                    // File already exists, keep retrying.
                    await Task.Delay(WaitForFileRetryDelay, cancellationToken);
                }
            }

            return false;
        }

        public static void ReleaseFileLock(
            string path)
        {
            File.Delete(path);
        }

#if MOTHBALL
        public static async Task<bool> LockAsync(
            string path, 
            IClock clock, 
            TimeSpan timeout, 
            object actionData,
            Func<FileStream, object, ValueTask> actionAsync,
            CancellationToken cancellationToken = default)
        {
            var timeoutAt = clock.UtcNow + timeout;

            while (clock.UtcNow >= timeoutAt)
            {
                while (File.Exists(path))
                {
                    if (clock.UtcNow >= timeoutAt)
                    {
                        return false;
                    }

                    await Task.Delay(WaitForFileRetryDelay, cancellationToken);
                }

                try
                {
                    using (var stream = File.Open(
                        path,
                        FileMode.CreateNew,
                        FileAccess.ReadWrite,
                        FileShare.Write))
                    {
                        await actionAsync(stream, actionData);

                        stream.Close();

                        File.Delete(path);

                        return true;
                    }
                }
                catch (IOException)
                {
                    await Task.Delay(WaitForFileRetryDelay, cancellationToken);
                }
            }

            return false;
        }
#endif
    }
}
