using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Lookups
{
    public interface IQueryUserLookup
    {
        UserSessionDetails Query(int sessionID);
    }
}