using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ServiceLogonMultifactor.Providers
{
    public class DnsHostProvider : IDnsHostProvider
    {
        public string GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Aggregate("", (current, ip) => current + (ip + " "));
        }
    }
}