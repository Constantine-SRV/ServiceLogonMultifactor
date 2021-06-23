using System;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel; //using System.Net.Http;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class TelegramTexts : ITelegramTexts, IStateAccessible
    {
        private readonly ITracing tracing;


        public TelegramTexts(ITracing tracing)
        {
            this.tracing = tracing;
        }

        public string TextForSend(string action, UserSessionDetails userSessionDetails)
        {
            // action: New connection ; Disconnected; Enabled

            var resText = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {Environment.NewLine}" +
                          $"<b>{action}</b> {TextBase(userSessionDetails)}";
            return resText;
        }

        public string TextBase(UserSessionDetails userSessionDetails)
        {
            // 
            var fromIpOrConsole = userSessionDetails.IP; //(userSession.isConsole ? "Console" : userSession.iP);
            var userTxt = userSessionDetails.IsConsole ? userSessionDetails.UserQuser : userSessionDetails.UserName;
            var resText =
                $"session {userSessionDetails.SessionID} on {Environment.MachineName} ({this.GetLocalIp()}){Environment.NewLine}" +
                $"from: {fromIpOrConsole} User: {userTxt}";
            return resText;
        }
    }
}