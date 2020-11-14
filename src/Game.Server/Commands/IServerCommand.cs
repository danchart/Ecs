using System.Threading.Tasks;

namespace Game.Server
{
    public interface IServerCommand<T>
    {
        bool CanExecute(GameServer server);

        Task<T> ExecuteAsync(GameServer gameServer);
    }
}
