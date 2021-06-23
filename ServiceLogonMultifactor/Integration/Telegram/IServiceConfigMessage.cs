namespace ServiceLogonMultifactor.Integration.Telegram
{
    public interface IServiceConfigMessage
    {
        void OnStartMessage();
        void OnStopMessage();
    }
}