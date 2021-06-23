namespace ServiceLogonMultifactor.Lookups
{
    public interface ISystemInfoLookup
    {
        string Query(string systemInfoFields = "");
    }
}