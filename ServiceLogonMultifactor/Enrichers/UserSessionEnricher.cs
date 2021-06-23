using System;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Enrichers
{
    public class UserSessionEnricher : IEnricher<UserSessionData>, IStateAccessible
    {
        public UserSessionData Enrich(UserSessionData userSessionData)
        {
            //надо полностью переписать и сделать все сильно проще
            //check if user has setting and fill missing from default
            var notDisconnectIP = "";
            var chatId = this.GetAppConfig().ChatId;
            var sendMessageBeforeDisconnectSec = this.GetAppConfig().SendMessageBeforeDisconnectSec;
            var waitForAnswerSec = this.GetAppConfig().WaitForAnswerSec;
            var userIndexInSettings = -1; //no such user in the settings
            var isAdmin = false;
            var canChangeIP = true;
            var disconnectIfNoAnswer = true;


            if (!string.IsNullOrEmpty(this.GetAppConfig().NotDisconnectIP)) notDisconnectIP = this.GetAppConfig().NotDisconnectIP;
            //search settings for current user
            for (var iU = 0; iU < this.GetAppConfig().UsersCollectionSection.UserConfigs.Count; iU++)
            {
                var userSettingsConfig = this.GetAppConfig().UsersCollectionSection.UserConfigs[iU];
                if (userSettingsConfig.Name.Equals(userSessionData.UserSessionDetails.UserQuser, StringComparison.OrdinalIgnoreCase))
                {
                    userIndexInSettings = iU;
                    // userInSettings = true;
                    if (!string.IsNullOrEmpty(userSettingsConfig.ChatId)) chatId = userSettingsConfig.ChatId;

                    disconnectIfNoAnswer = Convert.ToBoolean(userSettingsConfig.DisconnectIfNoAnswer);
                    isAdmin = Convert.ToBoolean(userSettingsConfig.IsAdmin);
                    canChangeIP = Convert.ToBoolean(userSettingsConfig.CanChangeIP);
                    if (userSettingsConfig.WaitForAnswerSec > 0) waitForAnswerSec = userSettingsConfig.WaitForAnswerSec;
                    if (userSettingsConfig.SendMessageBeforeDisconnectSec > 0)
                        sendMessageBeforeDisconnectSec = userSettingsConfig.SendMessageBeforeDisconnectSec;
                    if (userSettingsConfig.NotDisconnectIP != null)
                        notDisconnectIP =
                            userSettingsConfig
                                .NotDisconnectIP; //if null will use system if empty it is mean no IP permited
                }
            }

            var userConfig = new UserConfig
            {
                ChatId = chatId,
                Name = userSessionData.UserSessionDetails.UserQuser,
                NotDisconnectIP = notDisconnectIP,
                WaitForAnswerSec = waitForAnswerSec,
                SendMessageBeforeDisconnectSec = sendMessageBeforeDisconnectSec,
                DisconnectIfNoAnswer = disconnectIfNoAnswer,
                IsAdmin = isAdmin,
                CanChangeIP = canChangeIP
            };
            userSessionData.UserConfig = userConfig;
            userSessionData.UserIndexInSettings = userIndexInSettings;


            return userSessionData;
        }
    }
}