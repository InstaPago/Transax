using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Updater;
using System;
using System.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace InstaTransfer.ITService.Updater
{
    public partial class UpdaterService : ServiceBase
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

        public UpdaterService(string[] args)
        {
            InitializeComponent();
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
            double time;
            //Tomo el tiempo desde el app.config. Por defecto en 3m.
            try
            {
                time = int.Parse(ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyUpdaterTime]);
            }
            catch (Exception)
            {
                time = 180000;
            }

            timer.Interval = time;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            Logger.WriteSuccessLog(this.ServiceName + " inició correctamente.", GeneralResources.SourceNameUmbrellaUpdater);

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            GeneralHelper.KillProcess(UpdaterResources.UpdaterName);

            try
            {
                GeneralHelper.StartUpdater(ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyUpdaterFileName]);
                Logger.WriteSuccessLog(this.ServiceName + " ha sido completado.", GeneralResources.SourceNameUmbrellaUpdater);
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, GeneralResources.SourceNameUmbrellaUpdater);
            }          
        }

        protected override void OnContinue()
        {
            Logger.WriteSuccessLog(this.ServiceName + " ha sido resumido.", GeneralResources.SourceNameUmbrellaUpdater);
        }
    }
}
