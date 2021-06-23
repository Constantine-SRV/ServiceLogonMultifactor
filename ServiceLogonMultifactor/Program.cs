using System.ServiceProcess;

namespace ServiceLogonMultifactor
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new LogonMultifactorService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}