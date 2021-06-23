using System;
using System.Collections.Specialized;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Integration.Telegram
{
    public class TelegramButtons : ITelegramButtons, IStateAccessible
    {
        private readonly IHttpProvider httpProvider;
        private readonly ITelegramTexts telegramTexts;
        private readonly ITracing tracing;
        private string botId;

        public TelegramButtons(ITracing tracing, ITelegramTexts telegramTexts)
        {
            this.tracing = tracing;
            this.telegramTexts = telegramTexts;
            httpProvider = new HttpProvider(tracing);
        }

        public void SendButtons(UserSessionData userSessionData)
        {
            botId = this.GetAppConfig().BotId;
            var idRequest = userSessionData.IdRequest;
            var chatIdA = userSessionData.UserConfig.ChatId.Split(';');
            foreach (var chatId in chatIdA)
                try
                {
                    var text = "";
                    var btnMarkup = "";
                    text = TextForButtons(userSessionData);
                    btnMarkup = MarkupForButtons(userSessionData);
                    tracing.WriteFull(
                        $"SendButtons chatid: {chatId} markup: {btnMarkup} {Environment.NewLine} text: {text}");
                    var url = "https://api.telegram.org/bot" + botId + "/sendmessage";
                    var payload = "";
                    var values = new NameValueCollection
                    {
                        ["chat_id"] = chatId,
                        ["text"] = text,
                        ["parse_mode"] = "HTML",
                        ["reply_markup"] = btnMarkup
                    };
                    httpProvider.Post(url, values);
                    tracing.WriteFull($"SendButtons resp{payload} ");
                }
                catch (Exception e)
                {
                    tracing.WriteError($"error SendButtons {idRequest} {e.Message}");
                }
        }

        private string TextForButtons(UserSessionData userSessionData)
        {
            var text = "";
            try
            {
                var userSession = userSessionData.UserSessionDetails;
                var fromIpOrConsole = userSession.IsConsole ? "Console" : userSession.IP;
                var notDisconnectIP = "";
                if (userSessionData.UserIndexInSettings > -1 &&
                    this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP !=
                    null) //есть пользователь и не пустая строка
                    notDisconnectIP = this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP;
                else
                    notDisconnectIP = this.GetAppConfig().NotDisconnectIP;
                var sourceInTheList =
                    notDisconnectIP.IndexOf(fromIpOrConsole, StringComparison.CurrentCultureIgnoreCase) >= 0;
                tracing.WriteShort($"source in the list -{sourceInTheList}");
                tracing.WriteFull(
                    $"source:{fromIpOrConsole} List:{this.GetAppConfig().NotDisconnectIP} contains:{sourceInTheList}");
                if (sourceInTheList)
                    text = telegramTexts.TextForSend("New Connection", userSession) + Environment.NewLine +
                           $"the source {userSession.IP} is in \'Not disconnect\' list";
                else
                    text = telegramTexts.TextForSend("New Connection", userSession);
            }
            catch (Exception e)
            {
                tracing.WriteError($"error TextForButtons  {e.Message}");
            }

            return text;
        }

        private string MarkupForButtons(UserSessionData userSessionData)
        {
            /*1 выясняем sourceInTheList
             * 2 выясняем есть ли параметер NotDisconnectIP в оригинальном файле для этого пользователя
             * если нет то и кнопки всего две
             */
            var btnMarkup = "";
            try
            {
                var idRequest = userSessionData.IdRequest;
                var userSession = userSessionData.UserSessionDetails;
                var fromIpOrConsole = userSession.IsConsole ? "Console" : userSession.IP;
                var notDisconnectIP = "";
                var userAllowedToChangeIPList = false;
                if (userSessionData.UserIndexInSettings > -1) //if user in config file else using default
                {
                    userAllowedToChangeIPList = userSessionData.UserConfig.CanChangeIP;
                    if (this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP == null) //менять запрещено
                        notDisconnectIP = this.GetAppConfig().NotDisconnectIP;
                    else
                        notDisconnectIP = this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP;
                }

                var sourceInTheList =
                    notDisconnectIP.IndexOf(fromIpOrConsole, StringComparison.CurrentCultureIgnoreCase) >= 0;

                if (!userAllowedToChangeIPList) // without 3 button
                    btnMarkup = "{\"inline_keyboard\":[[{\"text\":\"disconnect\",\"callback_data\":\"" + idRequest +
                                "_D\"}," +
                                "{\"text\":\"Ok\",\"callback_data\":\"" + idRequest + "_E\"}]]}";
                else if (sourceInTheList) // enabled 3 buttons last Remove
                    btnMarkup = "{\"inline_keyboard\":[[{\"text\":\"disconnect\",\"callback_data\":\"" + idRequest +
                                "_D\"}," +
                                "{\"text\":\"Ok\",\"callback_data\":\"" + idRequest + "_E\"}," +
                                "{\"text\":\"disconnect & rem IP\",\"callback_data\":\"" + idRequest + "_R\"}]]}";
                else // enabled 3 buttons last Add
                    btnMarkup = "{\"inline_keyboard\":[[{\"text\":\"disconnect\",\"callback_data\":\"" + idRequest +
                                "_D\"}," +
                                "{\"text\":\"enable\",\"callback_data\":\"" + idRequest + "_E\"}," +
                                "{\"text\":\"enable & add IP\",\"callback_data\":\"" + idRequest + "_A\"}]]}";
            }
            catch (Exception e)
            {
                tracing.WriteError($"error MarkupForButtons  {e.Message}");
            }

            return btnMarkup;
        }
    }
}