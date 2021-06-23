using System.Collections.Specialized;
using System.Net;
using System.Text;
using ServiceLogonMultifactor.Logging;

namespace ServiceLogonMultifactor.Providers
{
    public class HttpProvider : IHttpProvider
    {
        private readonly ITracing tracing;

        public HttpProvider(ITracing tracing)
        {
            this.tracing = tracing;
        }

        public string Get(string url)
        {
            var payload = "error";
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072 | (SecurityProtocolType) 48 |
                                                   (SecurityProtocolType) 192 | (SecurityProtocolType) 768;
            payload = new TimedWebClientProxy {Timeout = 5000}.DownloadString(url);
           
            return payload;
        }

        public string Post(string url, NameValueCollection values)
        {
            var payload = "";
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072 | (SecurityProtocolType) 48 |
                                                   (SecurityProtocolType) 192 | (SecurityProtocolType) 768;


            using (var client = new WebClient())
            {
                var response = client.UploadValues(url, values);
                payload = Encoding.Default.GetString(response);
            }

            return payload;
        }
    }
}