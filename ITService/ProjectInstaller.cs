using InstaTransfer.ITLogic.Log;
using System;
using System.ComponentModel;
using System.Reflection;
using System.ServiceProcess;

namespace InstaTransfer.ITService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller serviceInstaller1;
        private ServiceInstaller serviceInstaller2;
        public ProjectInstaller()
        {
            InitializeComponent();

            try
            {
                process = new ServiceProcessInstaller();
                process.Account = ServiceAccount.User;

                serviceInstaller1 = new ServiceInstaller();
                serviceInstaller1.ServiceName = "ScraperService";
                serviceInstaller1.Description = "Corre el Scraper";
                serviceInstaller1.DisplayName = "Umbrella Scraper";
                serviceInstaller1.StartType = ServiceStartMode.Automatic;

                serviceInstaller2 = new ServiceInstaller();
                serviceInstaller2.ServiceName = "UpdaterService";
                serviceInstaller2.Description = "Corre el Updater";
                serviceInstaller2.DisplayName = "Umbrella Updater";
                serviceInstaller2.StartType = ServiceStartMode.Automatic;

                Installers.Add(process);
                Installers.Add(serviceInstaller1);
                Installers.Add(serviceInstaller2);
            }
            catch (Exception e)
            {

                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
