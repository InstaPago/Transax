using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace InstaTransfer.ITService.Scraper.Banesco
{
    public partial class SBanescoService : ServiceBase
    {
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

        public SBanescoService(string[] args)
        {
            InitializeComponent();

            string eventSourceName = this.ServiceName; 

            if (args.Count() > 0)
            {
                eventSourceName = args[0];
            }

            if (args.Count() > 1)
            {
                //logName = args[1];    Capturar argumentos
            }
            elBanesco = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSourceName, string.Empty);
            }
            elBanesco.Source = eventSourceName;
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Set up a timer to trigger every minute.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds  
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            elBanesco.WriteEntry(ServiceName + "Started successfully.");

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            elBanesco.WriteEntry(this.ServiceName + "Stopped successfully.");
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            try
            {
                //GeneralHelper.StartScraper(ServiceResources.ScraperFileName, GeneralHelper.GetBankIdString(Bank.Banesco));
                elBanesco.WriteEntry(this.ServiceName + " Scraper App started successfully.", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                elBanesco.WriteEntry(this.ServiceName + " Scraper App failed to start. " + e, EventLogEntryType.Error);
            }          
        }

        protected override void OnContinue()
        {
            elBanesco.WriteEntry(this.ServiceName + "Resumed.");
        }
    }
}
