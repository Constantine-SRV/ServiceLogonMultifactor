using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceLogonMultifactor.Providers
{
    public interface IWinApiUserAppRunProvider
    {
        UInt32 CreateProcessAsUser(string fileExe, string param, string workingDirectory, int dwSessionId);
    }
}
