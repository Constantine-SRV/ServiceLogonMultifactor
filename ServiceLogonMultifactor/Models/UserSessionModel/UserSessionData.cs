using System;
using ServiceLogonMultifactor.Configs.ApplicationConfig;

namespace ServiceLogonMultifactor.Models.UserSessionModel
{
    public class UserSessionData
    {
        public int SessionID { get; set; }
        public string IdRequest { get; set; }
        public DateTime SessionCreatedTimeStamp { get; set; }
        public UserConfig UserConfig { get; set; }
        public UserConfig UserConfigInTheFile { get; set; }
        public UserSessionDetails UserSessionDetails { get; set; }
        public int UserIndexInSettings { get; set; }
    }
}