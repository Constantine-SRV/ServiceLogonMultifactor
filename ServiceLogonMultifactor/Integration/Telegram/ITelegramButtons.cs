using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public interface ITelegramButtons
    {
        void SendButtons(UserSessionData userSessionData);
    }
}