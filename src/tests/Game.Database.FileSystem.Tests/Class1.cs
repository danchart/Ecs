using Common.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Test.Common;
using Xunit;

namespace Game.Database.FileSystem.Tests
{
    public class FileHelperTests
    {
        [Fact]
        public async Task LockAsync()
        {
            string path = $"{Path.GetTempPath()}\\{Guid.NewGuid():N}";

            TestClock clock = new TestClock(DateTime.Now);
            TimeSpan timeout = TimeSpan.FromSeconds(5);

            var lockFileTask = Task.Run(async () =>
            {
                using (var stream = File.Open(
                    path,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.Write))
                {
                    await Task.Delay(1000);
                    clock.DateTime = clock.DateTime + timeout + TimeSpan.FromSeconds(1);

                    stream.Close();

                    File.Delete(path);
                }
            });

            bool acquiredLock = false;

            var waitForLockTask = FileHelper.LockAsync(
                path,
                clock,
                timeout,
#pragma warning disable HAA0301 // Closure Allocation Source
                (stream) =>
#pragma warning restore HAA0301 // Closure Allocation Source
                {
                    acquiredLock = true;
                });

            await Task.WhenAll(new Task[] { lockFileTask, waitForLockTask });

            Assert.True(acquiredLock);
        }
    }
}
