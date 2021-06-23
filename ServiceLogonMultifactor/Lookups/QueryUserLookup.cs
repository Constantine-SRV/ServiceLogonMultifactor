using System;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Lookups
{
    public class QueryUserLookup : IQueryUserLookup, IStateAccessible
    {
        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly ITracing tracing;
        
        public QueryUserLookup(IExecuteCommandWrapper executeCommandWrapper, ITracing tracing)
        {
            this.executeCommandWrapper = executeCommandWrapper;
            this.tracing = tracing;
        }

        public UserSessionDetails Query(int sessionID)
        {
            var userSession = new UserSessionDetails();
            var linesCount = 0;
            
            try
            {
                //var a = ScStatic.sc.SESSIONNAME_fieldName;
                //USERNAME              SESSIONNAME        ID  STATE   IDLE TIME  LOGON TIME
//              > const                 console             1  Active    1 + 06:20  2021 - 05 - 13 0:15
                userSession.UserQuser = "-"; 
                var lines = executeCommandWrapper.Execute("quser.exe", sessionID.ToString());
                if (lines.Count > 0)
                {
                    if (lines[1].Contains("console")) userSession.IsConsole = true;
                    if (lines[1].Contains("...")) userSession.IsConsole = true; //connection from HyperV console
                    try
                    {
                        var headers = lines[0];

                        var logonTimePosition = headers.IndexOf(this.GetAppConfig().LOGON_TIME_fieldName);
                        var userNamePosition = headers.IndexOf(this.GetAppConfig().USERNAME_fieldName);
                        var sessionNamePosition = headers.IndexOf(this.GetAppConfig().SESSIONNAME_fieldName);
                        var idPosition = headers.IndexOf("ID");
                        var res = lines[1];
                        tracing.WriteFull(
                            $"logon time pos {logonTimePosition} UN pos {userNamePosition} Sess pos {sessionNamePosition} res {res}");
                        var userFromQuser = res.Substring(userNamePosition, sessionNamePosition - userNamePosition)
                            .Trim();
                        var dtFromQuser = res.Substring(logonTimePosition).Trim();
                        var sessionName = res.Substring(sessionNamePosition, idPosition - sessionNamePosition).Trim();
                        userSession.DTquser = dtFromQuser;
                        userSession.UserQuser = userFromQuser;
                        userSession.IP = sessionName; //id console is true no event log search.
                        userSession.Error = "";
                    }
                    catch (Exception e)
                    {
                        userSession.Error = "ERR string quser parsing  " + e.Message;
                        tracing.WriteError($"ERR string quser parsing lines count {lines.Count} message{e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                userSession.Error = "ERR process quser " + e.Message;
                tracing.WriteError($"ERR process quser  lines count {linesCount} message {e.Message}");
            }

            return userSession;
        }
    }
}