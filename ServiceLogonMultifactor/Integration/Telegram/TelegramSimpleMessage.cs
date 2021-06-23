using System;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class TelegramSimpleMessage : ITelegramSimpleMessage, IStateAccessible
    {
        private readonly IHttpProvider httpProvider;
        private readonly ITracing tracing;
        private string botId;

        public TelegramSimpleMessage(ITracing tracing)
        {
            this.tracing = tracing;
            httpProvider = new HttpProvider(tracing);
        }

        public void SendMessage(string chatIdSt, string text)
        {
            botId = this.GetAppConfig().BotId;
            text = text.Replace("#", "-").Replace("&", " "); //telegramm dosen't send #, &-new parameter
            var chatIdA = chatIdSt.Split(';');
            foreach (var chatId in chatIdA)
            {
                var url =
                    $"https://api.telegram.org/bot{botId}/sendMessage?chat_id={chatId}&parse_mode=HTML&text={text}";
                try
                {
                    tracing.WriteFull($"send message url {text}  ");
                    var payload = httpProvider.Get(url);

                    tracing.WriteFull($"send message response {payload}");
                }
                catch (Exception e)
                {
                    tracing.WriteError($"error SendMessage{text} url {url} error {e.Message}");
                }
            }
        }
    }
}