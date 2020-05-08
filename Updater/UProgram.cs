using System;
using InstaTransfer.ITResources.General;
using System.Linq;
using InstaTransfer.ITExceptions.Updater;
using System.Reflection;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.UpdaterContracts;
using InstaTransfer.UpdaterPresenter;
using InstaTransfer.ITLogic.Log;

namespace InstaTransfer.Updater
{
    class UProgram : IUContract
    {

        #region Presenter

        private static UPresenter _presenter;
        
        #endregion

        #region Variables

        public Object Updater
        {
            get { return this; }
        }

        #endregion

        /// <summary>
        /// Metodo main
        /// </summary>
        /// <param name="args">
        /// args[0]: filePath.
        /// args[1]: bankId.
        /// </param>
        static void Main(string[] args)
        {
            try
            {
                _presenter = new UPresenter();
                //UpdaterHelper.SetCulture(ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyDefaultCulture]);
                UpdaterHelper.SetCulture("es-VE");

                if (args.Count() == 0)
                {
                    _presenter.UpdateStatement();

                }
                else if (args.Count() == 2)
                {
                    _presenter.UpdateStatement(args[0], args[1]);
                }
            }
            catch (IndexOutOfRangeException e)
            {
                UException ex = new UException
                    (
                        GeneralErrors.IndexOutOfRangeExceptionCode,
                        typeof(UProgram).Name,
                        MethodBase.GetCurrentMethod().Name,
                        GeneralErrors.IndexOutOfRangeExceptionMessage,
                        e
                    );
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
                //GeneralHelper.ShowErrorMessage(ex.MessageException);
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }

}
