using System;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Configs.Services.Generic;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Configs.Services
{
    internal class UsersIpConfigManager : IUsersIpConfigManager, IStateAccessible
    {
        private readonly ITracing tracing;
        private readonly IConfigReader<LogonMultifactorConfig> configReader;
        private readonly IConfigWriter<LogonMultifactorConfig> configWriter;

        public UsersIpConfigManager(ITracing tracing,
            IConfigReader<LogonMultifactorConfig> configReader,
            IConfigWriter<LogonMultifactorConfig> configWriter)
        {
            this.tracing = tracing;
            this.configReader = configReader;
            this.configWriter = configWriter;
        }

        public void SyncUserIpInMemoryState()
        {
            var dtLastConfigRead = this.GetAppConfig().LastConfigRead;
            var dtLastWrite = configReader.GetLastWriteTime();
            if (dtLastWrite > dtLastConfigRead)
            {
                tracing.WriteShort("config read");
                this.GetAppState().AppConfig = configReader.ReadFromXmlFile();
                // ScStatic.sc.LastConfigRead = dtLastWrite;
            }
        }

        public void InsertIp(string iP, UserSessionData userSessionData)
        {
            try
            {
                if (userSessionData.UserIndexInSettings > -1)
                {
                    var currentList = this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP;
                    var newList = currentList + ";" + iP;
                    tracing.WriteFull(
                        $"new list add for user {userSessionData.UserConfig.Name} id: {userSessionData.UserIndexInSettings} list: {newList}");
                    this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP = newList;
                    this.GetAppConfig().ThreIsNewChanges = true;
                }
            }
            catch (Exception e)
            {
                tracing.WriteError($"error AddAndSaveIpList  {e.Message}");
            }
        }

        public void RemoveIp(string iP, UserSessionData userSessionData)
        {
            try
            {
                var currentList = this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP;
                var newList = "";
                var currentListArr = currentList.Split(';');
                for (var i = 0; i < currentListArr.Length; i++)
                    if (currentListArr[i] != iP)
                        newList += currentListArr[i] + ";";
                if (newList.Length > 0) newList = newList.Substring(0, newList.Length - 1); //cut last ;

                this.GetAppConfig().UsersCollectionSection.UserConfigs[userSessionData.UserIndexInSettings].NotDisconnectIP = newList;
                this.GetAppConfig().ThreIsNewChanges = true;
                tracing.WriteFull(
                    $"new list rem for user {userSessionData.UserConfig.Name} id: {userSessionData.UserIndexInSettings} list: {newList}");
            }
            catch (Exception e)
            {
                tracing.WriteError($"error RemAndSaveIpList  {e.Message}");
            }
        }

        public void UpsertUserIp()
        {
            /* 1 load config to new class
             * 2 if permited to save IP write a new IP
             * 3 save config
             
             */

            if (!this.GetAppConfig().ThreIsNewChanges) return;

            var logonMultifactorConfig = this.configReader.ReadFromXmlFile();
            foreach (var userConfig in logonMultifactorConfig.UsersCollectionSection.UserConfigs)
                if (userConfig.NotDisconnectIP != null)
                    foreach (var uCurrent in
                        this.GetAppConfig().UsersCollectionSection
                            .UserConfigs) //looking by name in case if new user was added to the middle of the user's list
                        if (userConfig.Name.Equals(uCurrent.Name, StringComparison.CurrentCultureIgnoreCase))
                            userConfig.NotDisconnectIP = uCurrent.NotDisconnectIP;
            logonMultifactorConfig.LastUpdateRecived = this.GetAppConfig().LastUpdateRecived;
            configWriter.WriteXml(logonMultifactorConfig);
            tracing.WriteShort("config saved");
        }
    }
}