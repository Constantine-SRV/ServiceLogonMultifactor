using System;

namespace ServiceLogonMultifactor.Providers
{
    public interface IFileSystemProvider
    {
        string ReadAllText(string fileName);
        DateTime GetLastWriteTime(string fileName);
        void WriteAllText(string fileName, string fileText);

        bool FileExists(string fileName);
    }
}