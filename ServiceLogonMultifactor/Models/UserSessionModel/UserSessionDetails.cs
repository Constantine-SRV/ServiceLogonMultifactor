namespace ServiceLogonMultifactor.Models.UserSessionModel
{
    public class UserSessionDetails
    {
        public int SessionID { get; set; }
        public string UserQuser { get; set; }
        public string UserName { get; set; }
        public string Domain { get; set; }
        public string DTquser { get; set; }
        public string DTEventLog { get; set; }
        public bool IsConsole { get; set; }
        public string Error { get; set; }
        public string IP { get; set; }
        public string EventLogTxt { get; set; }
        public string QuserTxt { get; set; }
        public string SesionChangeReason { get; set; }
        public string ExternalIP { get; set; }
        public string ExternalIPDetails { get; set; }
    }
}