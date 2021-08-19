using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;
using ServiceLogonMultifactor.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ServiceLogonMultifactor.Wrappers
{
   public class CheckSessionAndRunBlockerWraper: ICheckSessionAndRunBlockerWraper
    {
       
        private readonly ITracing tracing;
        private readonly IWinApiUserAppRunProvider winApiUserAppRunProvider;
        public CheckSessionAndRunBlockerWraper(ITracing tracing)
        {
            this.tracing = tracing;
            winApiUserAppRunProvider = new WinApiUserAppRunProvider(tracing);
        }
        public void ChecAndBlock(UserSessionData request)
        {

            if (request.InputBlockerProcessId<1 && request.ShouldBeBlocked)
            {
                int? waitForAnswerSec = request.UserConfig.WaitForAnswerSec;

                var fullPathtoBlocker = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "formBlockAccess.exe");
                var param = (waitForAnswerSec + 10).ToString(); 
                

                tracing.WriteFull($"ChecAndBlock fullPathtoBlocker= {fullPathtoBlocker}");
                request.InputBlockerProcessId = winApiUserAppRunProvider.CreateProcessAsUser(fullPathtoBlocker,param,AppDomain.CurrentDomain.BaseDirectory , request.SessionID);
               if (request.InputBlockerProcessId>0) request.ShouldBeMessaged = false;
                tracing.WriteFull($"ChecAndBlock blocker process id:{request.InputBlockerProcessId} ID:{request.IdRequest} session:{request.SessionID}");
            }

        }
        public void RemoveBlocker(UserSessionData request)
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                var procTokill = Process.GetProcessById((int)request.InputBlockerProcessId);

                procTokill.Kill();
                tracing.WriteFull($"Kill  process id= {request.InputBlockerProcessId}");
            }
            catch (Exception e)
            {
                request.InputBlockerProcessId = 0;
            }
        }
    }
}
