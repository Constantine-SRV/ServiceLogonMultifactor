using System;
using System.Net;

namespace ServiceLogonMultifactor.Providers
{
    public class TimedWebClientProxy : WebClient
    {
        public TimedWebClientProxy()
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