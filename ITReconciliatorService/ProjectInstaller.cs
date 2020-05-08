using InstaTransfer.ITLogic.Log;
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace InstaTransfer.ITReconciliatorService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller serviceInstaller1;
        public ProjectInstaller()
        {
            InitializeComponent();

            EventLogInstaller installer = FindInstaller(this.Installers);

            if (installer != null)
            {
                installer.Log = "Umbrella Log";
            }

            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.User;

            serviceInstaller1 = new ServiceInstaller();
            serviceInstaller1.ServiceName = "ReconciliatorService";
            serviceInstaller1.Description = "Corre el Conciliador";
            serviceInstaller1.DisplayName = "Umbrella Reconciliator";
            serviceInstaller1.StartType = ServiceStartMode.Automatic;

            Installers.Add(process);
            Installers.Add(serviceInstaller1);
        }


        private EventLogInstaller FindInstaller(InstallerCollection installers)
        {
            foreach (Installer installer in installers)
            {
                if (installer is EventLogInstaller)
                {
                    return (EventLogInstaller)installer;
                }

                EventLogInstaller eventLogInstaller = FindInstaller(installer.Installers);
                if (eventLogInstaller != null)
                {
                    return eventLogInstaller;
                }
            }
            return null;

        }
    }
}
