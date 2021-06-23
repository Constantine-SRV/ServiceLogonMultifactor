using System;
using System.Xml.Serialization;

namespace ServiceLogonMultifactor.Configs.ApplicationConfig
{
    [XmlRoot(ElementName = "ServiceConfig")]
    public class LogonMultifactorConfig : IConfigWithLastReadField
    {
        [XmlElement(ElementName = "Users")] 
        public UsersCollectionSection UsersCollectionSection { get; set; }

        [XmlAttribute(AttributeName = "botId")]
        public string BotId { get; set; } = "";

        [XmlAttribute(AttributeName = "chatId")]
        public string ChatId { get; set; } = "";

        [XmlAttribute(AttributeName = "DetailInfoOnServiceStart")]
        public bool DetailInfoOnServiceStart { get; set; } = true;

        [XmlAttribute(AttributeName = "daysOldDeliteLogs")]
        public int DaysOldDeliteLogs { get; set; }

        [XmlAttribute(AttributeName = "logsLevel")]
        public int LogsLevel { get; set; } = 15;

        [XmlAttribute(AttributeName = "waitForAnswerSec")]
        public int WaitForAnswerSec { get; set; } = 15;

        [XmlAttribute(AttributeName = "sendMessageBeforeDisconnectSec")]
        public int SendMessageBeforeDisconnectSec { get; set; } = 10;

        [XmlAttribute(AttributeName = "disconnectIfNoAnswer")]
        public string DisconnectIfNoAnswer { get; set; } = "true";

        [XmlAttribute(AttributeName = "notDisconnectIP")]
        public string NotDisconnectIP { get; set; } = "";

        [XmlAttribute(AttributeName = "CommandReadIntervalSec")]
        public int CommandReadIntervalSec { get; set; } = 30;

        [XmlAttribute(AttributeName = "SingleServiceOnTheBot")]
        public bool SingleServiceOnTheBot { get; set; }

        [XmlAttribute(AttributeName = "ClearTelegramRequestsAfterSeconds")]
        public int ClearTelegramRequestsAfterSeconds { get; set; } = 600;

        [XmlAttribute(AttributeName = "LinesPerSessionInTaskList")]
        public int LinesPerSessionInTaskList { get; set; } = 10;

        [XmlAttribute(AttributeName = "systemInfoOnStartFields")]
        public string SystemInfoOnStartFields { get; set; } = "";

        [XmlAttribute(AttributeName = "systemInfoOnEventFields")]
        public string SystemInfoOnEventFields { get; set; } = "";

        [XmlAttribute(AttributeName = "LOGON_TIME_fieldName")]
        public string LOGON_TIME_fieldName { get; set; } = "LOGON TIME";

        [XmlAttribute(AttributeName = "USERNAME_fieldName")]
        public string USERNAME_fieldName { get; set; } = "USERNAME";

        [XmlAttribute(AttributeName = "SESSIONNAME_fieldName")]
        public string SESSIONNAME_fieldName { get; set; } = "SESSIONNAME";

        [XmlAttribute(AttributeName = "LastUpdateRecived")]
        public long LastUpdateRecived { get; set; }

        [XmlAttribute(AttributeName = "ThreIsNewChanges")]
        public bool ThreIsNewChanges { get; set; }

        [XmlAttribute(AttributeName = "LastConfigRead")]
        public DateTime LastConfigRead { get; set; }
    }
}