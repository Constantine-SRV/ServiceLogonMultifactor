using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServiceLogonMultifactor.App;



namespace ServiceLogonMultifactor.Enrichers
{
    class UserSessionExternalIpEnricher : IUserSessionExternalIpEnricher, IStateAccessible
    {
        private readonly ITracing tracing;

        public UserSessionExternalIpEnricher(ITracing tracing)
        {
            this.tracing = tracing;
        }

        public UserSessionDetails Enrich(UserSessionDetails data)
        {

            if (!Task.Factory.StartNew(() => { RdgInfo(data); }).Wait(TimeSpan.FromMilliseconds(this.GetAppConfig().RdgVpnServersTimeoutMs)))
            {
                tracing.WriteError("RDG-VPN timeout ");
                data.ExternalIP = "timeout";
                data.ExternalIPDetails = "timeout RDG or VPN server answer";
            }
            return data;
        }
        private UserSessionDetails RdgInfo(UserSessionDetails data)
        {
            try
            {
                tracing.WriteFull($"Requesting RDG-VPN server");
                string serverIP = "";
                int serverPort = 91;
                string[] serverRdgListArr = this.GetAppConfig().RdgVpnServersList.Split(';');
                string[] serverIpWithPortArr = Array.Find(serverRdgListArr, e => e.Contains(data.IP)).Split(':');
                serverIP = serverIpWithPortArr[0];
                if (serverIpWithPortArr.Length>1) 
                    int.TryParse(serverIpWithPortArr[1], out serverPort);
                tracing.WriteFull($"RDG-VPN server {serverIP} port {serverPort}");
                
                TcpClient client = new TcpClient(serverIP, serverPort);
                NetworkStream nwStream = client.GetStream();
                //send
                string strsend = Environment.MachineName;
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(strsend);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                //read
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                string responseRDG = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                tracing.WriteFull($"recived from RDG-VPN server {responseRDG}");
                data.ExternalIP = responseRDG;
                client.Close();
            }
            catch(Exception e)
            {
                data.ExternalIP = e.Message;
            }
            return data;
        }
    }
}
