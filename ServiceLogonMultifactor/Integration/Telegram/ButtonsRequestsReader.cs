using System;
using ServiceLogonMultifactor.Configs.Services;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class ButtonsRequestsReader : IButtonsRequestsReader
    {
        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly ISystemInfoLookup systemInfoLookup;
        private readonly IButtonsRequestsProcessor requestProcessorCommand;
        private readonly ITelegramGetUpdates telegramGetUpdates;
        private readonly ITelegramSimpleMessage telegramSimpleMessage;
        private readonly ITelegramTexts telegramTexts;
        private readonly IUsersIpConfigManager usersIpConfigManager;
        private readonly ITracing tracing;
       
        public ButtonsRequestsReader(
            IUsersIpConfigManager usersIpConfigManager,
            ITracing tracing, 
            IExecuteCommandWrapper executeCommandWrapper, 
            ITelegramSimpleMessage telegramSimpleMessage,
            ITelegramTexts telegramTexts, 
            ITelegramGetUpdates telegramGetUpdates, 
            ISystemInfoLookup systemInfoLookup)
        {
            this.usersIpConfigManager = usersIpConfigManager;
            this.tracing = tracing;
            this.executeCommandWrapper = executeCommandWrapper;
            this.telegramSimpleMessage = telegramSimpleMessage;
            this.telegramTexts = telegramTexts;
            this.telegramGetUpdates = telegramGetUpdates;
            this.systemInfoLookup = systemInfoLookup;
          
            requestProcessorCommand =
                new ButtonsRequestsProcessor(tracing, executeCommandWrapper, telegramSimpleMessage, telegramTexts, systemInfoLookup);
        }

        public void CheckRequestsList()
        {
            /*1. check that not old
            * 2. check telea is the recuest in GetUpdate
            * 3. Processing according to the letter
            * 4. remove from the list
            * if timeout- disconnect (chk ip in the list)
            * add-rem IP
            * message.exe to the session
                     */
            try
            {
                for (var i = 0; i < LogonMultifactorService.ListRequest.Count; i++)
                {
                    var request = LogonMultifactorService.ListRequest[i];
                    var disconnectIfNoAnswer = false;
                    var notDisconnectIP = "";
                    int? waitForAnswerSec = request.UserConfig.WaitForAnswerSec;
                    int? sendMessageBeforeDisconnectSec = request.UserConfig.SendMessageBeforeDisconnectSec;
                    disconnectIfNoAnswer = request.UserConfig.DisconnectIfNoAnswer;
                    notDisconnectIP = request.UserConfig.NotDisconnectIP;
                    var secToDisconnect = (short) ((short) waitForAnswerSec - (DateTime.Now - request.SessionCreatedTimeStamp).TotalSeconds);
                    var fromIpOrConsole = request.UserSessionDetails.IsConsole ? "console" : request.UserSessionDetails.IP;
                    var sourceInTheList =
                        notDisconnectIP.IndexOf(fromIpOrConsole, StringComparison.CurrentCultureIgnoreCase) >= 0;
                    tracing.WriteFull($"waiting  {(DateTime.Now - request.SessionCreatedTimeStamp).TotalSeconds} msg " +
                                      $"{waitForAnswerSec - sendMessageBeforeDisconnectSec} disconnect {disconnectIfNoAnswer} in the list{sourceInTheList}");
                    if ((DateTime.Now - request.SessionCreatedTimeStamp).TotalSeconds >
                        waitForAnswerSec - sendMessageBeforeDisconnectSec && !sourceInTheList)
                    {
                        //send messade
                        var cmdres1 = executeCommandWrapper.ExecuteAndCollectOutput("msg",
                            $"{request.UserSessionDetails.SessionID} /time:1 Multifactor token was not received. " +
                            $"You session will be disconnect in {secToDisconnect} sec ");
                        tracing.WriteFull(
                            $"msg sent sess:{request.UserSessionDetails.SessionID}  sec:{secToDisconnect}  cmdres:{cmdres1}");
                    }

                    if ((DateTime.Now - request.SessionCreatedTimeStamp).TotalSeconds >= waitForAnswerSec)
                    {
                        //if no answer ....
                        if (disconnectIfNoAnswer && !sourceInTheList) requestProcessorCommand.Disconnect(request);
                        LogonMultifactorService.ListRequest.Remove(request);
                    }

                    var action = RequestProc(request.IdRequest);
                    if (action != "")
                    {
                        // (there is "data":" + idRequest in the answer
                        LogonMultifactorService.ListRequest.Remove(request);
                        switch (action)
                        {
                            case "D":
                                requestProcessorCommand.Disconnect(request);
                                break;
                            case "E":
                                requestProcessorCommand.Enable(request);
                                break;
                            case "A": // enable and  add ip
                                usersIpConfigManager.InsertIp(fromIpOrConsole, request);
                                requestProcessorCommand.Enable(request);

                                break;
                            case "R":
                                usersIpConfigManager.RemoveIp(fromIpOrConsole, request);
                                requestProcessorCommand.Disconnect(request);

                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                tracing.WriteError($"error CheckRequestsList  {e.Message}");
            }
        }


        private string RequestProc(string idRequest)
        {
            //read GetUpdate and if there is our iDRequest cut only letter (action) 
            
            var returnResult = "";
            var payload = telegramGetUpdates.GetUpdates();
            var pos = payload.IndexOf(",\"data\":\"" + idRequest);
            if (pos > 0)
            {
                var responseFull = payload.Substring(pos, idRequest.Length + 2 + 9);
                returnResult = responseFull.Substring(responseFull.Length - 1);
                tracing.WriteFull($"RequestProc action: {returnResult}{Environment.NewLine}payload {payload}");
            }

            return returnResult;
        }
    }
}