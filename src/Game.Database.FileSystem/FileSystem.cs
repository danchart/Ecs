using System;
using System.IO;
using System.Text;

namespace Game.Database.FileSystem
{
    /// <summary>
    /// Basic base path file system wrapper. 
    /// </summary>
    public class FileSystem : IFileSystem
    {
        private readonly string _basePath;

        public FileSystem(string root)
        {
            _basePath = root ?? throw new ArgumentNullException(nameof(root));
        }

        public bool Exists(string path)
        {
            return File.Exists(GetFullPath(path));
        }

        public string Read(string path)
        {
            return File.ReadAllText(GetFullPath(path));
        }

        public void Write(string path, string contents)
        {
            File.WriteAllText(GetFullPath(path), contents, Encoding.UTF8);
        }

        private string GetFullPath(string path)
        {
            return $"{this._basePath}\\{path}";
        }
    }
}
