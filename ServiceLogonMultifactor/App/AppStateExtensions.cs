using System;
using ServiceLogonMultifactor.Configs.ApplicationConfig;

namespace ServiceLogonMultifactor.App
{
    public static class AppStateExtensions
    {
        public static AppState GetAppState(this IStateAccessible context)
        {
            return AppState.GetCurrent();
        }

        public static LogonMultifactorConfig GetAppConfig(this IStateAccessible context)
        {
            return AppState.GetCurrent().AppConfig;
        }

        public static string GetLocalIp(this IStateAccessible context)
        {
            return AppState.GetCurrent().LocalIP;
        }

        public static DateTime GetStartTime(this IStateAccessible context)
        {
            return AppState.GetCurrent().StartTime;
        }
    }
}