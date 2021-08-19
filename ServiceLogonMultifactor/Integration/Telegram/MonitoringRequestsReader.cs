using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Models.TelegramModel;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class MonitoringRequestsReader : IMonitoringRequestsReader, IStateAccessible
    {
        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly ISystemInfoLookup systemInfoLookup;
        private readonly ITaskListLookup taskListLookup;
        private readonly IMonitoringRequestsProcessor monitoringRequestsProcessor;
        private readonly ITracingRender tracingRender;
        private readonly ITelegramGetUpdates telegramGetUpdates;
        private readonly ITelegramSimpleMessage telegramSimpleMessage;
        private readonly ITracing tracing;

        public MonitoringRequestsReader(ITracing tracing, ITelegramGetUpdates telegramGetUpdates,
            ITelegramSimpleMessage telegramSimpleMessage,
            IExecuteCommandWrapper executeCommandWrapper, ISystemInfoLookup systemInfoLookup, ITracingRender tracingRender)
        {
            this.tracing = tracing;
            this.telegramGetUpdates = telegramGetUpdates;
            this.executeCommandWrapper = executeCommandWrapper;
            this.telegramSimpleMessage = telegramSimpleMessage;
            this.systemInfoLookup = systemInfoLookup;
            taskListLookup = new TaskListLookup(tracing, executeCommandWrapper);
            this.tracingRender = tracingRender;
            monitoringRequestsProcessor = new MonitoringRequestsProcessor(tracing, telegramSimpleMessage, executeCommandWrapper,
                systemInfoLookup, tracingRender);
        }


        public void ReadRequest()
        {
            var maxRequestProcessed = this.GetAppConfig().LastUpdateRecived;
            var minimumDtClearRequests = DateTime.Now.AddSeconds(this.GetAppConfig().ClearTelegramRequestsAfterSeconds * -1);
            long updateIdToClear = 0;
            var result = "error";
            if (this.GetAppConfig().SingleServiceOnTheBot)
                result = telegramGetUpdates.GetUpdates(); //maxRequestProcessed+1); //delete requests < maxRequestProcessed
            else
                result = telegramGetUpdates.GetUpdates();
            if (result == "error") return;

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(result)))
            {
                var deserializer = new DataContractJsonSerializer(typeof(Root));
                var telegaAnswer = (Root) deserializer.ReadObject(ms);
                tracing.WriteFullFull($"responce deserialized {telegaAnswer.result.Count}");
                foreach (var r in telegaAnswer.result)
                    if (r.message != null &&
                        r.message.text != null) //we are looking only messages buttons and etc -> ignore
                    {
                        var chatId = r.message.chat.id;
                        var textMessage = r.message.text;
                        var dateUnix = r.message.date;
                        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        dt = dt.AddSeconds(dateUnix).ToLocalTime();
                        if (dt < minimumDtClearRequests)
                        {

                            updateIdToClear = r.update_id;
                            tracing.WriteFull($"clear update {updateIdToClear + 1} dt:{dt.ToString():HH:mm:ss:f} {r.message.text}");
                        }
                        tracing.WriteFull($"processing from {chatId} update:{r.update_id} text {textMessage}");
                        if (r.update_id > maxRequestProcessed) 
                        {
                            tracing.WriteFull($"checking RequestID:{r.update_id} is our");
                            var containsPC =
                                textMessage.IndexOf(Environment.MachineName, StringComparison.OrdinalIgnoreCase) >= 0;
                            var containsAll = textMessage.IndexOf("all", StringComparison.OrdinalIgnoreCase) >= 0;
                            if (containsAll || containsPC)
                            {
                                //send answer to requester
                                tracing.WriteFull($"RequestID:{r.update_id}  {r.message.text}  will be processed");
                                Task.Factory.StartNew(() =>
                                {
                                    monitoringRequestsProcessor.ProcessRequest(chatId, dt, textMessage);
                                }).Wait(TimeSpan.FromSeconds(10));
                            }
                            else
                            {
                                tracing.WriteFull(
                                    $"RequestID:{r.update_id}  {r.message.text}  is not contain my name or all");
                            }

                            this.GetAppConfig().LastUpdateRecived = r.update_id;
                            this.GetAppConfig().ThreIsNewChanges = true;
                        }
                    }

                if (updateIdToClear > 0)
                {
                    tracing.WriteFull($"clear update {updateIdToClear + 1}");
                    telegramGetUpdates.GetUpdates(updateIdToClear + 1);
                }
            }
        }
    }
}