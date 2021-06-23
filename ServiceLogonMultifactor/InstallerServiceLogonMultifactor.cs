using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServiceLogonMultifactor
{
    [RunInstaller(true)]
    public partial class InstallerServiceLogonMultifactor : Installer
    {
        private readonly ServiceProcessInstaller processInstaller;
        private readonly ServiceInstaller serviceInstaller;

        public InstallerServiceLogonMultifactor()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();
            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "ServiceLogonMultifactor"; // 
            serviceInstaller.DisplayName = "ServiceLogonMultifactor";
            serviceInstaller.Description =
                "Service for logging of logon and unlock events and send notification to telegram bot";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }

        private void RetrieveServiceName()
        {
            //https://stackoverflow.com/questions/1704554/any-way-to-override-net-windows-service-name-without-recompiling

            var serviceName = Context.Parameters["servicename"];
            if (!string.IsNullOrEmpty(serviceName))
            {
                serviceInstaller.ServiceName = serviceName;
                serviceInstaller.DisplayName = serviceName;
            }

            var serviceDescription = Context.Parameters["description"];
            if (!string.IsNullOrEmpty(serviceName)) serviceInstaller.Description = serviceDescription;
        }

        public override void Install(IDictionary stateSaver)
        {
            RetrieveServiceName();
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            RetrieveServiceName();
            base.Uninstall(savedState);
        }
    }
}