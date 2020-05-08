using InstaTransfer.ITService.Scraper;
using InstaTransfer.ITService.Updater;
using System.ServiceProcess;

namespace InstaTransfer.ITService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ScraperService(args),
                new UpdaterService(args)
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
