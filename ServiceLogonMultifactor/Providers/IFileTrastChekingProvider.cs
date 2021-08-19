namespace ServiceLogonMultifactor.Providers
{
    public interface IFileTrastChekingProvider
    {
        bool FileBlockValid(string fileExec);
    }
}