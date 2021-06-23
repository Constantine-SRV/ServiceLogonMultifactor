using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Enrichers
{
    public interface IUserSessionEventLogEnricher : IEnricher<UserSessionDetails>
    {
       
    }
}