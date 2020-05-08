using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ITChargeAccountScraperService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            #region Variables

            SBanescoPEService service = new SBanescoPEService();

            #endregion

            /// Corre en modo full service al iniciar desde consola
            /// Corre en modo normal service al iniciar desde el Service Controller
            /// <source>https://alastaircrabtree.com/how-to-run-a-dotnet-windows-service-as-a-console-app/</source>
            if (Environment.UserInteractive)
            {
                service.RunAsConsole(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { service };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
