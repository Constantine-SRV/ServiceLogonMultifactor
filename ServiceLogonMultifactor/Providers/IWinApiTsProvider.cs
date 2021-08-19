using System.Collections.Generic;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Providers
{
    public interface IWinApiTsProvider
    {
        IEnumerable<LogonSession> GetSessions();
    }
}