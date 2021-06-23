namespace ServiceLogonMultifactor.Integration.Telegram
{
    public interface ITelegramGetUpdates
    {
        string GetUpdates(long offset = 0);
    }
}