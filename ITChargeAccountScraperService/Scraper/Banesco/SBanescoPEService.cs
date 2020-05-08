using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Service.ScraperPE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ITChargeAccountScraperService
{
    public partial class SBanescoPEService : ServiceBase
    {
        #region Variables
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        #endregion

        #region Constructor
        public SBanescoPEService()
        {
            InitializeComponent();
            
            // Ties the EventLog member of the ServiceBase base class to the
            // ServiceExample event log created when the service was installed.
            EventLog.Log = "Umbrella Log";
        }
        #endregion

        #region Callbacks
        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Set up a timer to trigger n minutes.  
            System.Timers.Timer timer = new System.Timers.Timer();
            double time;
            //Tomo el tiempo desde el app.config. Por defecto en 1m.
            try
            {
                time = int.Parse(ConfigurationManager.AppSettings[ScraperPEServiceResources.AppSettingsKeyScraperPETime]);
            }
            catch (Exception)
            {
                time = 60000;
            }

            timer.Interval = time;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            Logger.WriteSuccessLog(this.ServiceName + " inició correctamente.", "Umbrella Scraper PE");

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            Logger.WriteSuccessLog(this.ServiceName + " termino correctamente.", "Umbrella Scraper PE");
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            try
            {
                // Cerramos los Scrapers que se quedaran guindados
                KillProcess(ScraperPEServiceResources.ScraperName);

                // Corremos el Scraper
                RunScraperPE(ConfigurationManager.AppSettings[ScraperPEServiceResources.AppSettingsKeyScraperPEFileName]);

                Logger.WriteSuccessLog(this.ServiceName + " ha sido completado.", "Umbrella Scraper PE");
            }
            catch (Exception e)
            {
                // Error
                Logger.WriteErrorLog(e, "Umbrella Scraper PE");
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Detiene inmediatamente el proceso con el nombre especificado
        /// </summary>
        /// <param name="processName">El nombre del proceso</param>
        public static void KillProcess(string processName)
        {
            foreach (Process proc in Process.GetProcessesByName(processName))
            {
                proc.Kill();
            }
        }

        /// <summary>
        /// Levanta el scraper
        /// </summary>
        /// <param name="filePath">Ruta del ejecutable del scraper</param>
        public static void RunScraperPE(string filePath)
        {
            Process scraper = new Process();
            scraper.StartInfo.FileName = filePath;
            scraper.Start();
        }

        /// <summary>
        /// Corre el servicio en modo consola
        /// </summary>
        /// <param name="args"></param>
        /// <source>https://alastaircrabtree.com/how-to-run-a-dotnet-windows-service-as-a-console-app/</source>
        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            OnStop();
        }

        #endregion
    }
}
