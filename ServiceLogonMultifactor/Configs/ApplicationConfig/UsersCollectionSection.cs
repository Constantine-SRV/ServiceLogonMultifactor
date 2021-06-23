using System.Collections.Generic;
using System.Xml.Serialization;

namespace ServiceLogonMultifactor.Configs.ApplicationConfig
{
    [XmlRoot(ElementName = "Users")]
    public class UsersCollectionSection
    {
        [XmlElement(ElementName = "user")] 
        public List<UserConfig> UserConfigs { get; set; }
    }
}