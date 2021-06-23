using System;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.App
{
    public class AppState
    {
        private AppState()
        {
        }
        
        private static readonly AppState Instance = new AppState();

        public static AppState GetCurrent()
        {
            return Instance;
        }
        
        public LogonMultifactorConfig AppConfig { get; set; }
        public string LocalIP { get; } = new DnsHostProvider().GetLocalIp();
        public DateTime StartTime { get; } = DateTime.Now;
    }
}