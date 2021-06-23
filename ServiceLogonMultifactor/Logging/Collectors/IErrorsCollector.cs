namespace ServiceLogonMultifactor.Logging.Collectors
{
    public interface IErrorsCollector
    {
        string Collect(int days = 0);
    }
}