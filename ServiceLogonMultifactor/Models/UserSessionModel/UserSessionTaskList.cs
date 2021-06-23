namespace ServiceLogonMultifactor.Models.UserSessionModel
{
    public class UserSessionTaskList
    {
        public string ImageName { get; set; }
        public string SessionName { get; set; }
        public int SessionId { get; set; }
        public string MemUsageString { get; set; }
        public int MemUsageInt { get; set; }
    }
}