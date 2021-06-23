using System.Collections.Specialized;

namespace ServiceLogonMultifactor.Providers
{
    public interface IHttpProvider
    {
        string Get(string url);
        string Post(string url, NameValueCollection values);
    }
}