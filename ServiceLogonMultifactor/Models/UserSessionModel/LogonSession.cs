namespace ServiceLogonMultifactor.Models.UserSessionModel
{
    public class LogonSession
    {
        public int SessionID { get; set; }
        public string SessionState { get; set; }
        public string WorkstationName { get; set; }
        public string IP { get; set; }
        public string UserName { get; set; }
        public string Domain { get; set; }
        public string DisplayResolution { get; set; }
    }
}