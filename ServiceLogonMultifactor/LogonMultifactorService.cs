using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Configs.Services;
using ServiceLogonMultifactor.Configs.Services.Generic;
using ServiceLogonMultifactor.Enrichers;
using ServiceLogonMultifactor.Integration.Telegram;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Logging.Infrastructure;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Models.UserSessionModel;
using ServiceLogonMultifactor.Providers;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor
{
    public partial class LogonMultifactorService : ServiceBase, IStateAccessible
    {
        private static readonly object LockObjectCheckResponse = new object(); //для таймера
        private static readonly object LockObjectConfigAndCommand = new object(); //для таймера
        public static List<UserSessionData> ListRequest = new List<UserSessionData>();
        private static int eventNumber; //events count
        private readonly IFileSystemProvider fileSystemProvider;
        private readonly IMonitoringRequestsReader answerOnMonitoringRequest;
        private readonly IEnricher<UserSessionData> userSessionEnricher;
        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly IUsersIpConfigManager usersIpConfigManager;
        private readonly IWinApiProvider winApiProvider;
        private readonly ISystemInfoLookup systemInfoLookup;
        private readonly IOldLogsCleaner oldLogsCleaner;
        private readonly IAppWorkers regularTsks;
        private readonly IButtonsRequestsReader requestsProcessor;
        private readonly IUserSessionEventLogEnricher userSessionEventLogEnricher;
        private readonly IQueryUserLookup iqUserLookup;
        private readonly IEnricher<UserSessionDetails> userSessionDetailsEnricher;
        private readonly IServiceConfigMessage serviceConfigMessage;
        private readonly ITracingRender tracingRender;
        private readonly ISleepProvider sleepProvider; //FakeSleepProvider();//  
        private readonly ITelegramButtons telegramButtons;
        private readonly ITelegramSimpleMessage telegramSimpleMessage;
        private readonly ITracing tracing;
        private Timer timerCheckResponse;
        private Timer timerConfigAndCommand;

        public LogonMultifactorService()
        {
            CanHandleSessionChangeEvent = true;
            InitializeComponent();
            oldLogsCleaner = new OldLogsCleaner();
            oldLogsCleaner.DeleteOldLogs(true, true);
            tracing = new Tracing(new TracingFoldersConfigurator());
            fileSystemProvider = new FileSystemProvider();
            var configReader = new ConfigReader<LogonMultifactorConfig>(tracing, fileSystemProvider);
            var configWriter = new ConfigWriter<LogonMultifactorConfig>(fileSystemProvider);
            usersIpConfigManager = new UsersIpConfigManager(tracing, configReader, configWriter);
            
            this.GetAppState().AppConfig = configReader.ReadFromXmlFile();
            executeCommandWrapper = new ExecuteCommandWrapper(tracing, fileSystemProvider);
            var telegramTexts = new TelegramTexts(tracing);
            var telegramGetUpdates = new TelegramGetUpdates(tracing);
            telegramSimpleMessage = new TelegramSimpleMessage(tracing);
            telegramButtons = new TelegramButtons(tracing, telegramTexts);
            sleepProvider = new SleepProvider();
            iqUserLookup = new QueryUserLookup(executeCommandWrapper, tracing);
            userSessionEventLogEnricher = new UserSessionEventLogEnricher(tracing);
            winApiProvider = new WinApiProvider();
            userSessionDetailsEnricher = new UserSessionDetailsEnricher(tracing, winApiProvider);
            systemInfoLookup = new SystemInfoLookup(tracing, executeCommandWrapper);
            
            requestsProcessor = new ButtonsRequestsReader(usersIpConfigManager,tracing, executeCommandWrapper, telegramSimpleMessage, telegramTexts,
                telegramGetUpdates, systemInfoLookup);
            tracingRender = new TracingRender(tracing);
            answerOnMonitoringRequest = new MonitoringRequestsReader(tracing, telegramGetUpdates, telegramSimpleMessage,
                executeCommandWrapper, systemInfoLookup, tracingRender);
            regularTsks = new AppWorkers(usersIpConfigManager, tracing, answerOnMonitoringRequest);
            serviceConfigMessage =
                new ServiceConfigMessage(tracing, telegramSimpleMessage, systemInfoLookup, tracingRender);
            userSessionEnricher = new UserSessionEnricher();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                timerCheckResponse = new Timer {Enabled = true, Interval = 1 * 1000, AutoReset = true};
                timerCheckResponse.Elapsed += TimerCheckResponseElapsed;
                timerCheckResponse.Start();
                timerConfigAndCommand = new Timer
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(this.GetAppConfig().CommandReadIntervalSec).TotalMilliseconds,
                    AutoReset = true
                };
                timerConfigAndCommand.Elapsed += TimerConfigAndCommand;
                timerConfigAndCommand.Start();
                Task.Factory.StartNew(() => { serviceConfigMessage.OnStartMessage(); });
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(e.Message + e.StackTrace);
            }
        }

        protected override void OnStop()
        {
            // Task.Factory.StartNew(() => {serviceConfigMessage.OnStopMessage();});
            serviceConfigMessage.OnStopMessage();
        }

        private void TimerCheckResponseElapsed(object sender, ElapsedEventArgs e)
        {
            lock (LockObjectCheckResponse)
            {
                timerCheckResponse.Stop();
                if (!Task.Factory.StartNew(() => { requestsProcessor.CheckRequestsList(); })
                    .Wait(TimeSpan.FromSeconds(20))) tracing.WriteError("TimerCheckResponseElapsed timeout ");
                timerCheckResponse.Start();
            }
        }

        private void TimerConfigAndCommand(object sender, ElapsedEventArgs e)
        {
            lock (LockObjectConfigAndCommand)
            {
                timerConfigAndCommand.Stop();
                if (!Task.Factory.StartNew(() => { regularTsks.StartOneMinuteProcess(); }).Wait(TimeSpan.FromSeconds(30)))
                    tracing.WriteError("TimerConfigAndCommand timeout ");
                timerCheckResponse.Start();


                timerConfigAndCommand.Start();
            }
        }

        protected override void OnSessionChange(SessionChangeDescription desc)
        {
            tracing.WriteShort($"session {desc.SessionId.ToString()} {desc.Reason}");
            switch (desc.Reason)
            {
                case SessionChangeReason.SessionLogon:
                case SessionChangeReason.RemoteConnect:
                case SessionChangeReason.SessionUnlock:
                    try
                    {
                        eventNumber++;
                        var userSession = new UserSessionDetails();

                        userSession = iqUserLookup.Query(desc.SessionId);
                        if (userSession.UserQuser !=
                            "-") //if there is no user from quser - it is a phantom that appears before the normal logon. 
                        {
                            userSession.SessionID = desc.SessionId;
                            userSession.SesionChangeReason = desc.Reason.ToString();
                            userSession.EventLogTxt = "";
                            userSession = userSessionDetailsEnricher.Enrich(userSession);

                            //on this string, we will look for the answer in Telegram 
                            var idRequest = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Environment.MachineName +
                                            "_" + userSession.SessionID;
                            var alreadyProcessed = false;
                            for (var i = 0; i < ListRequest.Count; i++)
                            {
                                var request1 = ListRequest[i];
                                if (request1.SessionID == desc.SessionId &&
                                    (DateTime.Now - request1.SessionCreatedTimeStamp).TotalSeconds < 5)
                                {
                                    alreadyProcessed = true;
                                    tracing.WriteFull($"event{i.ToString()} already processed");
                                    break;
                                }
                            }

                            if (!alreadyProcessed)
                            {
                                UserSessionData userSessionData;
                                userSessionData = new UserSessionData();
                                userSessionData.SessionID = userSession.SessionID;
                                userSessionData.IdRequest = idRequest;
                                userSessionData.UserSessionDetails = userSession;
                                userSessionData.SessionCreatedTimeStamp = DateTime.Now;
                                userSessionData = userSessionEnricher.Enrich(userSessionData);
                                ListRequest.Add(
                                    userSessionData); //we have added a request to the list and now we will check by timer if there is a response to this request
                                telegramButtons.SendButtons(userSessionData);
                                tracing.WriteFull(tracingRender.RenderUserSessionData(userSessionData, eventNumber));
                            }
                        }
                        else
                        {
                            tracing.WriteFull($"fantom session {desc.SessionId}");
                        }
                    }
                    catch (Exception e)
                    {
                        tracing.WriteError($"event{eventNumber} Error case event {e.Message}");
                    }

                    break;
            }

            base.OnSessionChange(desc);
        }
    }
}