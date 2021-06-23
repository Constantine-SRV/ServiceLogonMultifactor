using System;
using System.Linq;
using System.Reflection;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Logging.Collectors;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class MonitoringRequestsProcessor : IMonitoringRequestsProcessor, IStateAccessible
    {
        private readonly IErrorsCollector errorsCollector;

        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly ISystemInfoLookup systemInfoLookup;
        private readonly ITaskListLookup taskListLookup;
        private readonly ITracingRender tracingRender;
        private readonly ITelegramSimpleMessage telegramSimpleMessage;
        private readonly ITracing tracing;

        public MonitoringRequestsProcessor(ITracing tracing, ITelegramSimpleMessage telegramSimpleMessage,
            IExecuteCommandWrapper executeCommandWrapper, ISystemInfoLookup systemInfoLookup, ITracingRender tracingRender)
        {
            this.tracing = tracing;
            this.executeCommandWrapper = executeCommandWrapper;
            this.telegramSimpleMessage = telegramSimpleMessage;
            this.systemInfoLookup = systemInfoLookup;
            taskListLookup = new TaskListLookup(tracing, executeCommandWrapper);
            this.tracingRender = tracingRender;
            errorsCollector = new ErrorsCollector();
        }

        public void ProcessRequest(int chatId, DateTime dt, string textMessage)
        {
            /* 1- check chatId admin
            * 2- check tsdisconnect or other commands
           * 3- execute all commands in the line
                      */

            var disconPos = textMessage.IndexOf("dis", StringComparison.OrdinalIgnoreCase);
            var containsInfo = textMessage.IndexOf("inform", StringComparison.OrdinalIgnoreCase) >= 0;
            var containsSys = textMessage.IndexOf("sys", StringComparison.OrdinalIgnoreCase) >= 0;
            var containsQuser = textMessage.IndexOf("quser", StringComparison.OrdinalIgnoreCase) >= 0;
            var containsTask = textMessage.IndexOf("task", StringComparison.OrdinalIgnoreCase) >= 0;
            var containsConfig = textMessage.IndexOf("config", StringComparison.OrdinalIgnoreCase) >= 0;
            var containsVersion = textMessage.IndexOf("ver", StringComparison.OrdinalIgnoreCase) >= 0;
            var containsErrorsLog = textMessage.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;
            var isAdmin = checkIfAdmin(chatId); //null -not in the list
            tracing.WriteFull(
                $" request {textMessage} IsAdmin:{isAdmin} IsInConfig:{isAdmin.HasValue} info:{containsInfo} Dis:{disconPos}");
            try
            {
                if (disconPos >= 0 && isAdmin == true)
                {
                    /*from the position +1 looking space or +2 in case if sessionid > 100
                                  * remove letters or spaces */
                    var resMessage = "";
                    var sesionIdPos = textMessage.IndexOf(" ", disconPos);
                    if (sesionIdPos < 0) return;
                    var lenOfSessionId = 3;
                    if (sesionIdPos + lenOfSessionId > textMessage.Length)
                        lenOfSessionId = textMessage.Length - sesionIdPos;
                    var sessionId = "0";
                    var sessionIdInt = 0;
                    sessionId = new string(textMessage.Substring(sesionIdPos, lenOfSessionId)
                        .Where(c => char.IsDigit(c)).ToArray());
                    int.TryParse(sessionId, out sessionIdInt);
                    if (sessionIdInt > 0) //all parsing,removing letters and etc is Ok we have some int value
                    {
                        executeCommandWrapper.ExecuteAndCollectOutput("tsdiscon", sessionIdInt.ToString());
                        var quserRes = executeCommandWrapper.ExecuteAndCollectOutput("quser.exe", "");
                        resMessage =
                            $"command tsdiscon {sessionId} was executed on {Environment.MachineName} {Environment.NewLine} {quserRes}";
                        tracing.WriteShort($"dis by:{chatId} {resMessage}");
                        telegramSimpleMessage.SendMessage(chatId.ToString(), resMessage);
                    }
                    else
                    {
                        tracing.WriteShort($"dis faled:{chatId} {sessionIdInt}");
                    }
                }

                if (containsInfo && isAdmin == true)
                {
                    var quserRes = executeCommandWrapper.ExecuteAndCollectOutput("quser.exe", "");
                    var systeminfo = systemInfoLookup.Query(this.GetAppConfig().SystemInfoOnStartFields);
                    var taskList = taskListLookup.Query(this.GetAppConfig().LinesPerSessionInTaskList);
                    tracing.WriteShort($"info for:{chatId}  sent");
                    var finalText = $"{Environment.MachineName}/{this.GetLocalIp()}{Environment.NewLine}" + quserRes +
                                    Environment.NewLine + systeminfo + Environment.NewLine + taskList;
                    telegramSimpleMessage.SendMessage(chatId.ToString(), finalText);
                }

                if (containsSys && isAdmin == true)
                {
                    var systeminfo = systemInfoLookup.Query(this.GetAppConfig().SystemInfoOnStartFields);
                    tracing.WriteShort($"systemInfo for:{chatId}  sent");
                    var finalText = $"{Environment.MachineName}/{this.GetLocalIp()}{Environment.NewLine}" + systeminfo;
                    telegramSimpleMessage.SendMessage(chatId.ToString(), finalText);
                }

                if (containsQuser && isAdmin == true)
                {
                    var quserRes = executeCommandWrapper.ExecuteAndCollectOutput("quser.exe", "");
                    tracing.WriteShort($"quser for:{chatId}  sent");
                    var finalText = $"{Environment.MachineName}/{this.GetLocalIp()}{Environment.NewLine}" + quserRes;
                    telegramSimpleMessage.SendMessage(chatId.ToString(), finalText);
                }

                if (containsTask && isAdmin == true)
                {
                    var taskList = taskListLookup.Query(this.GetAppConfig().LinesPerSessionInTaskList);
                    tracing.WriteShort($"tasklist for:{chatId}  sent");
                    var finalText = $"{Environment.MachineName}/{this.GetLocalIp()}{Environment.NewLine}" + taskList;
                    telegramSimpleMessage.SendMessage(chatId.ToString(), finalText);
                }

                if (containsVersion && isAdmin == true)
                {
                    var v = Assembly.GetExecutingAssembly().GetName().Version;
                    var version =
                        $"Version  {v.Major}.{v.Minor}.{v.Build}.{v.Revision} uptime {(DateTime.Now - this.GetStartTime()).TotalHours.ToString("0.00")} hours";
                    
                    tracing.WriteShort($"version for:{chatId}  sent");
                    var finalText =
                        $"{Environment.MachineName}/{this.GetLocalIp()}{Environment.NewLine}{version}{Environment.NewLine}{HealthStatisticCollector.GetCurrent().GetOkFailRatioAsText()} ";
                    telegramSimpleMessage.SendMessage(chatId.ToString(), finalText);
                }

                if (containsConfig && isAdmin == true)
                {
                    tracing.WriteShort($"config for:{chatId}  sent");
                    var finalText =
                        $"{Environment.MachineName}/{this.GetLocalIp()} config:{Environment.NewLine}{tracingRender.RenderConfig(this.GetAppConfig())}";
                    telegramSimpleMessage.SendMessage(chatId.ToString(), finalText);
                }

                if (containsErrorsLog && isAdmin == true)
                {
                    tracing.WriteShort($"errors for:{chatId}  sent");
                    var finalText =
                        $"{Environment.MachineName}/{this.GetLocalIp()} Errors:{Environment.NewLine} {errorsCollector.Collect()}";
                    telegramSimpleMessage.SendMessage(chatId.ToString(), finalText);
                }

                if (isAdmin == false && isAdmin.HasValue)
                {
                    tracing.WriteShort($"info for:{chatId} - not Admin");
                    telegramSimpleMessage.SendMessage(chatId.ToString(),
                        $"sorry you aren't admin on {Environment.MachineName}/{this.GetLocalIp()}");
                }
            }
            catch (Exception e)
            {
                tracing.WriteError($"ProcessRequest {e.Message}");
            }
        }

        private bool? checkIfAdmin(int chatId)
        {
            //only in the class we need to check if the chatID in common settings or user settings or not exists
            bool? result = null;
            var ChatIDa = this.GetAppConfig().ChatId.Split(';');
            if (Array.Exists(ChatIDa, E => E == chatId.ToString())) return true;

            foreach (var u in this.GetAppConfig().UsersCollectionSection.UserConfigs)
            {
                ChatIDa = u.ChatId.Split(';');
                if (Array.Exists(ChatIDa, E => E == chatId.ToString()))
                {
                    result = u.IsAdmin;
                    break;
                }
            }

            return result;
        }
    }
}