using System;
using System.Diagnostics.Eventing.Reader;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Enrichers
{
    internal class UserSessionEventLogEnricher : IUserSessionEventLogEnricher
    {
        private readonly ITracing tracing;

        public UserSessionEventLogEnricher(ITracing tracing)
        {
            this.tracing = tracing;
        }

        
        public UserSessionDetails Enrich(UserSessionDetails data)
        {
            try
            {

                var dt = DateTime.Now;
                if (DateTime.TryParse(data.DTquser, out dt))
                    if ((DateTime.Now - dt).TotalDays > 300) 
                        dt = DateTime.Now;

                //[(EventID=21 or EventID=25)]]</
                // string query = $"*[System/EventID={eventId}] and *[UserData/EventXML/SessionID = {userSession.sessionID.ToString()}]";

                var startTime = dt.AddMinutes(-5).ToUniversalTime().ToString("o");
                //var endTime = dt.AddMinutes(10).ToUniversalTime().ToString("o");
                var query =
                    $"(*[System/EventID=25] or *[System/EventID=21]) and *[System[TimeCreated[@SystemTime >= '{startTime}']]]"
                    //  + $" and *[System[TimeCreated[@SystemTime <= '{endTime}']]]"
                    + $" and *[UserData/EventXML/SessionID = {data.SessionID.ToString()}]";

                tracing.WriteFull($"search sting   {query}");
                var logType = "Microsoft-Windows-TerminalServices-LocalSessionManager/Operational";
                //query = $"*[System/EventID=21] and *[UserData/EventXML/SessionID = {2}]";

                var elQuery = new
                    EventLogQuery(logType, PathType.LogName, query);
                elQuery.ReverseDirection = true;

                var elReader = new EventLogReader(elQuery);
                var eventInstance = elReader.ReadEvent();
                if (eventInstance != null)

                {
                   
                    data.DTEventLog = eventInstance.TimeCreated.ToString();
                    if (eventInstance.Properties.Count == 3)
                    {
                        data.UserName = eventInstance.Properties[0].Value.ToString();
                        data.IP = eventInstance.Properties[2].Value.ToString();
                    }

                    foreach (var p in eventInstance.Properties)
                    {
                        data.EventLogTxt += p.Value + Environment.NewLine;
                        tracing.WriteFull($"event log {p.Value}");
                       
                    }

              
                }
            }
            catch (Exception e)
            {
                tracing.WriteError($" event log reader {e.Message}");
            }

            return data;
        }
    }
}