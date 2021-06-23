using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Configs.Services
{
    public interface IUsersIpConfigManager
    {
        void UpsertUserIp();
        
        void SyncUserIpInMemoryState();
        
        void InsertIp(string iP, UserSessionData userSessionData);
        
        void RemoveIp(string iP, UserSessionData userSessionData);
    }
}