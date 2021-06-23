namespace ServiceLogonMultifactor.Logging.Infrastructure
{
    public interface IOldLogsCleaner
    {
        void DeleteOldLogs(bool ShortLogDelete = false, bool errorLogDelete = false, bool fullLogDelete = true);
    }
}