using System;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public interface IMonitoringRequestsProcessor
    {
        void ProcessRequest(int chatId, DateTime dt, string textMessage);
    }
}