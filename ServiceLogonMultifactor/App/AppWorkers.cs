using System;
using System.Threading.Tasks;
using ServiceLogonMultifactor.Configs.Services;
using ServiceLogonMultifactor.Integration.Telegram;
using ServiceLogonMultifactor.Logging;

namespace ServiceLogonMultifactor.App
{
    public class AppWorkers : IAppWorkers
    {
        private readonly IMonitoringRequestsReader answerOnMonitoringRequest;

        private readonly IUsersIpConfigManager usersIpConfigManager;
        private int timerCount;

        public AppWorkers(
            IUsersIpConfigManager usersIpConfigManager,
            ITracing tracing, 
            IMonitoringRequestsReader answerOnMonitoringRequest)
        {
            this.usersIpConfigManager = usersIpConfigManager;
            this.answerOnMonitoringRequest = answerOnMonitoringRequest;
        }

        public void StartOneMinuteProcess()
        {
            timerCount++;
    
            Task.Factory.StartNew(() => { answerOnMonitoringRequest.ReadRequest(); }).Wait(TimeSpan.FromSeconds(20));
            Task.Factory.StartNew(() =>
            {
                usersIpConfigManager.UpsertUserIp();
                usersIpConfigManager.SyncUserIpInMemoryState();
            }).Wait(TimeSpan.FromSeconds(10));

            if (timerCount > 10)
                //not frequent tasks: clear logs, etc
                timerCount = 0;
        }
    }
}