namespace ServiceLogonMultifactor.Integration.Telegram
{
    public interface ITelegramSimpleMessage
    {
        void SendMessage(string chatId, string text);
    }
}