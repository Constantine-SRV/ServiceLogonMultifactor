using System;
using System.Linq;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Enrichers
{
    public class UserSessionDetailsEnricher : IEnricher<UserSessionDetails>
    {
        private readonly IWinApiTsProvider winApiProvider;
        private readonly ITracing tracing;

        public UserSessionDetailsEnricher(ITracing tracing, IWinApiTsProvider winApiProvider)
        {
            this.tracing = tracing;
            this.winApiProvider = winApiProvider;
        }

        public UserSessionDetails Enrich(UserSessionDetails userSessionDetails)
        {
            try
            {
                var listSessions = winApiProvider.GetSessions();
                var s = listSessions.First(x => x.SessionID == userSessionDetails.SessionID);
                tracing.WriteFull(
                    $"WinApiTs {s.SessionID} {s.IP} {s.Domain}\\{s.UserName} {s.WorkstationName} {s.SessionState}");
                userSessionDetails.UserName = s.UserName;
                userSessionDetails.Domain = s.Domain;
                userSessionDetails.IP = s.IP;
                if (userSessionDetails.IsConsole) userSessionDetails.IP = "console";
            }
            catch (Exception e)
            {
                tracing.WriteError($"error SessionInfo WinAPI {e.Message}");
            }

            return userSessionDetails;
        }
    }
}