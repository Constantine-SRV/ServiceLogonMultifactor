using System;
using System.Reflection;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class ServiceConfigMessage : IServiceConfigMessage, IStateAccessible
    {
        private readonly ISystemInfoLookup systemInfoLookup;
        private readonly ITracingRender tracingRender;
        private readonly ITelegramSimpleMessage telegramSimpleMessage;
        private readonly ITracing tracing;

        public ServiceConfigMessage(ITracing tracing, ITelegramSimpleMessage telegramSimpleMessage,
            ISystemInfoLookup systemInfoLookup, ITracingRender tracingRender)
        {
            this.tracing = tracing;
            this.telegramSimpleMessage = telegramSimpleMessage;
            this.systemInfoLookup = systemInfoLookup;
            this.tracingRender = tracingRender;
        }

        public void OnStartMessage()
        {
            try
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                var text =
                    $"{DateTime.Now:HH:mm:ss} {Environment.MachineName} ({this.GetLocalIp()}){Environment.NewLine}" +
                    $"Service  {v.Major}.{v.Minor}.{v.Build}.{v.Revision} <b>STARTED</b>{Environment.NewLine}";

                telegramSimpleMessage.SendMessage(this.GetAppConfig().ChatId, text);
                tracing.WriteShort(text);
                text = $"{systemInfoLookup.Query(this.GetAppConfig().SystemInfoOnStartFields)}" +
                       $"<b>Settings</b> {Environment.NewLine}{tracingRender.RenderConfig(this.GetAppConfig())}";
                tracing.WriteFull(text);
                if (this.GetAppConfig().DetailInfoOnServiceStart) telegramSimpleMessage.SendMessage(this.GetAppConfig().ChatId, text);
            }
            catch (Exception e)
            {
                tracing.WriteError($"Error OnStartMessage {e.Message}");
            }
        }

        public void OnStopMessage()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            var text =
                $"{DateTime.Now:HH:mm:ss} {Environment.MachineName} ({this.GetLocalIp()}){Environment.NewLine}" +
                $"Service {v.Major}.{v.Minor}.{v.Build}.{v.Revision} <b>STOPPED</b> ";
            tracing.WriteShort("Service stopped");
            telegramSimpleMessage.SendMessage(this.GetAppConfig().ChatId, text);
        }
    }
}