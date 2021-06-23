using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public interface ITelegramTexts
    {
        string TextForSend(string action, UserSessionDetails userSessionDetails);
        string TextBase(UserSessionDetails userSessionDetails);
    }
}