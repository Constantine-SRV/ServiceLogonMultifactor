using System.Xml.Serialization;

namespace ServiceLogonMultifactor.Configs.ApplicationConfig
{
    [XmlRoot(ElementName = "user")]
    public class UserConfig
    {
        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "chatId")]
        public string ChatId { get; set; }

        [XmlAttribute(AttributeName = "waitForAnswerSec")]
        public int WaitForAnswerSec { get; set; }

        [XmlAttribute(AttributeName = "sendMessageBeforeDisconnectSec")]
        public int SendMessageBeforeDisconnectSec { get; set; }

        [XmlAttribute(AttributeName = "disconnectIfNoAnswer")]
        public bool DisconnectIfNoAnswer { get; set; } = true;

        [XmlAttribute(AttributeName = "notDisconnectIP")]
        public string NotDisconnectIP { get; set; } = "";

        //https://stackoverflow.com/questions/10511835/xml-serialization-error-on-bool-types bool True->true
        [XmlAttribute(AttributeName = "CanChangeIP")]
        public bool CanChangeIP { get; set; }

        [XmlAttribute(AttributeName = "IsAdmin")]
        public bool IsAdmin { get; set; }
    }
}