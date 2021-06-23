using System;
using System.IO;

namespace ServiceLogonMultifactor.Providers
{
    public class FileSystemProvider : IFileSystemProvider
    {
        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public DateTime GetLastWriteTime(string fileName)
        {
            return File.GetLastWriteTime(fileName);
        }

        public void WriteAllText(string fileName, string fileText)
        {
            File.WriteAllText(fileName, fileText);
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}