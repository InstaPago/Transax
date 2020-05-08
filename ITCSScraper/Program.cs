using BLL.Concrete;
using CefSharp;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.General;
using InstaTransfer.ITExceptions.Scraper.ScraperPEBanesco;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper.ScraperBanescoPE;
using ITCSScraper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InstaTransfer.ITCSS
{
    static class Program
    {
        static private AE_ArchivoBLL _AEABLL;
        static AE_ArchivoBLL AEABLL
        {
            get
            {
                if (_AEABLL == null)
                {
                    return new AE_ArchivoBLL();
                }
                else
                {
                    return _AEABLL;
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.WriteSuccessLog("Entro al Main", "ITCSS (Program)");

            //Variables
            UUserBLL UUBLL = new UUserBLL();
            List<Task> tasks = new List<Task>();
            UserType _userType;
            List<AE_Archivo> totalFiles;

            try
            {
                // Verificamos si hay conexion a internet
                if (!GeneralHelper.IsConnectedToInternet())
                {
                    throw new NoInternetConnectionException(GeneralErrors.NoInternetConnectionExceptionMessage);
                }

                // Buscamos el total de los archivos pendientes
                totalFiles = AEABLL.GetFilesByStatus(new List<ChargeAccountStatus> {
                    ChargeAccountStatus.Accepted,
                    ChargeAccountStatus.Pending,
                    ChargeAccountStatus.Uploaded
                });

                // Verificamos que existan archivos pendientes por procesar
                if (totalFiles.Count == 0)
                {
                    throw new NoPendingFilesException(ScraperBanescoPEErrors.NoPendingFilesExceptionMessage);
                }

                // Asignamos el tipo de usuario
                _userType = AssignUserType();

                // Buscamos los usuarios activos
                List<UUser> allActiveUsers = UUBLL.GetUsers(UmbrellaUserStatus.Active, _userType);

                // Retornamos el primero de cada banco/razon social
                var umbrellaUsers = allActiveUsers.GroupBy(u => new { u.IdUSocialReason, u.IdUBank }).Select(y => y.First()).ToList();

                // Verificamos que existan usuarios activos
                if (allActiveUsers.Count == 0 || allActiveUsers == null)
                {
                    throw new UmbrellaUserNotFoundException(GeneralErrors.UserNotFoundExceptionMessage);
                }

                //// Agrupamos las solicitudes por usuario
                //var requestGroups = allPendingRequests.GroupBy(u => new { u.Rif, u.IdUBank }).Select(g => g.ToList()).ToList();

                //For Windows 7 and above, best to include relevant app.manifest entries as well
                Cef.EnableHighDPISupport();

                //We're going to manually call Cef.Shutdown below, this maybe required in some complex scenarios
                CefSharpSettings.ShutdownOnExit = false;

                var settings = new CefSettings()
                {
                    BrowserSubprocessPath = ConfigurationManager.AppSettings["CefSharpSubprocessPath"],
                    CachePath = ""
                };
                //Perform dependency check to make sure all relevant resources are in our output directory.
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

                // Recorremos las solicitudes y levantamos los scrapers
                foreach (var user in umbrellaUsers)
                {
                    Bank bank = GeneralHelper.GetBankEnum(user.IdUBank);
                    UserType userType = (UserType)user.IdUUserType;

                    //Configuracion del task
                    var task = Task.Factory.StartNew(() =>
                    {
                        // Levantamos el scraper correspondiente segun el banco
                        switch (bank)
                        {
                            case Bank.Provincial:
                                {
                                    Application.Run(new SBrowserProvincial(user));
                                    break;
                                }
                            case Bank.Banesco:
                                {
                                    // Levantamos el scraper correspondiente segun el tipo de plataforma en linea
                                    switch (userType)
                                    {
                                        case UserType.OnlineBanking:
                                            {
                                                Application.Run(new SBrowserBanesco(user));
                                                break;
                                            }
                                        case UserType.MassiveBankingUploads:
                                            {
                                                Application.Run(new SBrowserBanescoPEUploads(user));
                                                break;
                                            }
                                        case UserType.MassivaBankingConsults:
                                            {
                                                Application.Run(new SBrowserBanescoPEConsults(user));
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    break;
                                }
                        }
                    });
                    tasks.Add(task);
                }
                // Esperamos a que todos los tasks terminen
                Task.WaitAll(tasks.ToArray());
                // Cerramos Cef
                Cef.Shutdown();
            }

            catch (NoInternetConnectionException e)
            {
                Logger.WriteErrorLog(e.MessageException, "ITCSScraper Main");
            }
            catch (UmbrellaUserNotFoundException e)
            {
                Logger.WriteWarningLog(e.MessageException, "ITCSScraper Main");
            }
            catch (NoPendingFilesException e)
            {
                Logger.WriteWarningLog(e.MessageException, "ITCSScraper Main");
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, "ITCSScraper Main");
            }
        }

        /// <summary>
        /// Asigna el tipo de usuario de la plataforma bancaria segun los archivos de cargo en cuenta
        /// </summary>
        /// <returns>Tipo de usuario a utilizar</returns>
        static UserType AssignUserType()
        {
            // Variables
            List<AE_Archivo> pendingFiles = AEABLL.GetFilesByStatus(ChargeAccountStatus.Pending);

            // Verificamos si existen archivos pendientes y devolvemos el tipo de usuario correspondiente
            if (pendingFiles.Count == 0)
            {
                return UserType.MassivaBankingConsults;
            }
            else if (pendingFiles.Count > 0)
            {
                return UserType.MassiveBankingUploads;
            }
            else
            {
                return UserType.MassivaBankingConsults;
            }
        }
    }
}
