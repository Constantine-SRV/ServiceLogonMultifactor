using System;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Models.UserSessionModel;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class ButtonsRequestsProcessor : IButtonsRequestsProcessor, IStateAccessible
    {
        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly ISystemInfoLookup systemInfoLookup;
        private readonly ITelegramSimpleMessage telegramSimpleMessage;
        private readonly ITelegramTexts telegramTexts;
        private readonly ITracing tracing;

        public ButtonsRequestsProcessor(ITracing tracing, IExecuteCommandWrapper executeCommandWrapper,
            ITelegramSimpleMessage telegramSimpleMessage, ITelegramTexts telegramTexts, ISystemInfoLookup systemInfoLookup)
        {
            this.tracing = tracing;
            this.executeCommandWrapper = executeCommandWrapper;
            this.telegramSimpleMessage = telegramSimpleMessage;
            this.telegramTexts = telegramTexts;
            this.systemInfoLookup = systemInfoLookup;
        }

        public void Disconnect(UserSessionData userSessionData)
        {
            var cmdres1 = executeCommandWrapper.ExecuteAndCollectOutput("tsdiscon", userSessionData.UserSessionDetails.SessionID.ToString());
            var cmdres2 = executeCommandWrapper.ExecuteAndCollectOutput("quser", "");
            var textD = telegramTexts.TextForSend("Disconnect", userSessionData.UserSessionDetails) + Environment.NewLine +
                        (cmdres1.Length < 1 ? "" : cmdres1 + Environment.NewLine);
            if (userSessionData.UserConfig.IsAdmin)
                textD += Environment.NewLine + systemInfoLookup.Query(this.GetAppConfig().SystemInfoOnEventFields);
            telegramSimpleMessage.SendMessage(userSessionData.UserConfig.ChatId, textD);
        }

        public void Enable(UserSessionData userSessionData)
        {
            var textE = telegramTexts.TextForSend("Enabled", userSessionData.UserSessionDetails) + Environment.NewLine;
            if (userSessionData.UserConfig.IsAdmin)
                textE +=
                    $"{executeCommandWrapper.ExecuteAndCollectOutput("quser", "")}{Environment.NewLine}{systemInfoLookup.Query(this.GetAppConfig().SystemInfoOnEventFields)}";
            telegramSimpleMessage.SendMessage(userSessionData.UserConfig.ChatId, textE);
        }
    }
}