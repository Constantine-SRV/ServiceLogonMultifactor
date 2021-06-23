using System;
using System.Collections.Generic;
using System.Net;

namespace ConsoleReadTelegramBot
{
    public class HttpWorker
    {
        public string GetUpdates(string botId, long offset = 0)
        {
            var listExeption = new List<Exception>();
            var errorTime = new List<string>();
            var url = $"https://api.telegram.org/bot{botId}/getUpdates";
            if (offset > 0) url += "?offset=" + offset;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072 | (SecurityProtocolType) 48 |
                                                   (SecurityProtocolType) 192 | (SecurityProtocolType) 768;
            try
            {
                return new TimedWebClient {Timeout = 5000}.DownloadString(url);
            }
            catch (Exception e)
            {
                return "ErrorGetUpdate" + e.Message;
            }
        }
    }

    public class TimedWebClient : WebClient
    {
        public TimedWebClient()
        {
            Timeout = 60 * 1000;
        }

      
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072 | (SecurityProtocolType) 48 |
                                                   (SecurityProtocolType) 192 | (SecurityProtocolType) 768;
            var objWebRequest = base.GetWebRequest(address);
            objWebRequest.Timeout = Timeout;
            return objWebRequest;
        }
    }
}