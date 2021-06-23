using System.Collections.Generic;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Providers
{
    public interface IWinApiProvider
    {
        IEnumerable<LogonSession> GetSessions();
    }
}