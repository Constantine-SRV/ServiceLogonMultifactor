namespace ServiceLogonMultifactor.Lookups
{
    public interface ITaskListLookup
    {
        string Query(int linesPerSession);
    }
}