using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Logging.Collectors;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class TelegramGetUpdates : ITelegramGetUpdates
    {
        private readonly IHttpProvider httpProvider;
        private readonly ITracing tracing;
        private string botId;

        public TelegramGetUpdates(ITracing tracing)
        {
            this.tracing = tracing;
            httpProvider = new HttpProvider(tracing);
        }

        public string GetUpdates(long offset = 0)
        {
            botId = AppState.GetCurrent().AppConfig.BotId;
            var payload = "error";
            const int maxAttempt = 5; //to settings
            var currentAttempt = 0;
            var listExceptions = new List<Exception>();
            var errorTime = new List<string>();
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072 | (SecurityProtocolType) 48 |
                                                       (SecurityProtocolType) 192 | (SecurityProtocolType) 768;
                var url = $"https://api.telegram.org/bot{botId}/getUpdates";
                if (offset > 0) url += "?offset=" + offset;
                while (currentAttempt < maxAttempt)
                    try
                    {
                        payload = httpProvider.Get(url);
                        HealthStatisticCollector.GetCurrent().ReportOk();
                        break;
                    }
                    catch (Exception e)
                    {
                        HealthStatisticCollector.GetCurrent().ReportFailed();
                        errorTime.Add($"{DateTime.Now:HH:mm:ss.fff}");
                        listExceptions.Add(e);
                        currentAttempt++;
                        if (currentAttempt == maxAttempt) break;
                        Thread.Sleep(2000); //to seetings
                    }

                if (currentAttempt > 3)
                    tracing.WriteErrorFull($"GetUpdates count {currentAttempt} {Environment.NewLine}" +
                                           $"{string.Join(Environment.NewLine, errorTime)} ");
                if (currentAttempt >= maxAttempt) throw new AggregateException("max attemp reached", listExceptions);
                tracing.WriteFullFull($"GetUpdates response lenght:{payload.Length}");
            }
            catch (AggregateException e)
            {
                HealthStatisticCollector.GetCurrent().ReportFaileMax();
              //  tracing.WriteError($"error GetUpdates {string.Join(Environment.NewLine ,e.InnerExceptions.Select(s=>s.Message)) }");
            }
            catch (Exception e)
            {
                tracing.WriteError($"error GetUpdates {e.Message}");
            }

            return payload;
        }
    }
}