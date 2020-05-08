using BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.General;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.General;
using InstaTransfer.ScraperPresenter;
using InstaTransfer.ScraperView.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace InstaTransfer.Scraper
{
    static class SProgram
    {
        private static SPresenter _presenter;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            // Variables
            UUserBLL UUBLL = new UUserBLL();
            _presenter = new SPresenter();

            try
            {
                // Verificamos si hay conexion a internet
                if (!GeneralHelper.IsConnectedToInternet())
                {
                    throw new NoInternetConnectionException(GeneralErrors.NoInternetConnectionExceptionMessage);
                }
                //Verificamos el estado del proxy por el puerto especificado
                FiddlerHelper.CheckProxy(8888);

                //Buscamos los usuarios activos
                List<UUser> allActiveUsers = UUBLL.GetUsers(UmbrellaUserStatus.Active, UserType.OnlineBanking);

                //Retornamos el primero de cada banco/razon social
                var umbrellaUsers = allActiveUsers.GroupBy(u => new { u.IdUSocialReason, u.IdUBank }).Select(y => y.First()).ToList();

                // Verificamos que existan usuarios activos
                if (allActiveUsers.Count == 0 || allActiveUsers == null)
                {
                    throw new UmbrellaUserNotFoundException(GeneralErrors.UserNotFoundExceptionMessage);
                }
                // Por cada usuario activo corremos el scraper en un hilo diferente respectivamente
                foreach (var user in umbrellaUsers)
                {
                    var th = new Thread(() =>
                    {
                        var scraper = ScraperFactory.GetScraper(user);
                        Application.Run(scraper);
                    });
                    th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }
            }
            catch (NoInternetConnectionException e)
            {
                Logger.WriteErrorLog(e.MessageException, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (UmbrellaUserNotFoundException e)
            {
                Logger.WriteWarningLog(e.MessageException, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
