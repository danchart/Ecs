﻿using Common.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Database.FileSystem
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

        public static async Task<bool> LockAsync(
            string path, 
            IClock clock, 
            TimeSpan timeout, 
            Action<FileStream> action,
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
                        action(stream);

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
    }
}
