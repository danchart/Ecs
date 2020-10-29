using System.Threading.Tasks;

namespace Game.Database.FileSystem
{
    public interface IFileSystem
    {
        bool Exists(string path);

        Task<bool> CreateAsync(string path);

        string Read(string path);
        void Write(string path, string contents);
    }
}
