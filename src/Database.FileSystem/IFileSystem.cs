using System.Threading.Tasks;

namespace Database.FileSystem
{
    public interface IFileSystem
    {
        bool Exists(string path);

        string Read(string path);
        void Write(string path, string contents);
    }
}
