using ServiceLogonMultifactor.Models.UserSessionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceLogonMultifactor.Wrappers
{
    public interface ICheckSessionAndRunBlockerWraper
    {
        void ChecAndBlock(UserSessionData request);
        void RemoveBlocker(UserSessionData request);
    }
}
