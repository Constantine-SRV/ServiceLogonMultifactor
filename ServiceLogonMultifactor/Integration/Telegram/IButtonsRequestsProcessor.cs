using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public interface IButtonsRequestsProcessor
    {
        void Disconnect(UserSessionData userSessionData);
        void Enable(UserSessionData userSessionData);
    }
}