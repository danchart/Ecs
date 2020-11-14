using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Server
{
    public sealed class GameServerCommander
    {
        readonly GameServer _gameServer;

        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public GameServerCommander(GameServer server)
        {
            _gameServer = server ?? throw new ArgumentNullException(nameof(server));
        }

        public async Task<T> RunCommandAsync<T>(IServerCommand<T> command)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (command.CanExecute(_gameServer))
                {
                    return await command.ExecuteAsync(_gameServer);
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
