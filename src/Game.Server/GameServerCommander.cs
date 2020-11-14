using Game.Server.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Server
{
    public sealed class GameServerCommander
    {
        readonly GameServer _server;

        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public GameServerCommander(GameServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public async Task<T> RunCommandAsync<T>(IServerCommand<T> command)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (command.CanExecute())
                {
                    return await command.ExecuteAsync(_server);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return (T)default;
        }
    }
}
