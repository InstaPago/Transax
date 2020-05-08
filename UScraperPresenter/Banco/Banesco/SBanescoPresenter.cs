using Fiddler;
using InstaTransfer.ITExceptions.Scraper;
using InstaTransfer.ITExceptions.Scraper.Banesco;
using InstaTransfer.ITLogic;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITResources.Scraper.ScraperBanesco;
using InstaTransfer.ScraperContracts.Banco.Banesco;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.Constants;

namespace InstaTransfer.ScraperPresenter.Banco.Banesco
{
    /// <summary>
    /// Define variables, inicializacion, y metodos exclusivos para el <see cref="ISBanescoContract"/>
    /// </summary>
    public class SBanescoPresenter : SPresenter
    {
        #region Variables
        private ISBanescoContract _view;
        private enum AccountField { UserBFI, PWBFI, UserTD, PWTD }

        int _count = 0;
        #endregion

        #region Constructor
        public SBanescoPresenter(ISBanescoContract viewUScraperBanesco) :
            base(viewUScraperBanesco)
        {
            _view = viewUScraperBanesco;
            _view.WbUmbrellaExplorer.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wbUmbrellaExplorer_DocumentCompleted);
            _view.TsmiLogout.Click += new EventHandler(tsmiLogout_Click);
            _view.TsbLogin.Click += new EventHandler(tsbLogin_Click);
            _view.SBrowser.Load += new EventHandler(Browser_Load);
            _view.TsbHome.Click += new EventHandler(tsbHome_Click);
            _view.WbUmbrellaExplorer.NewWindow += new System.ComponentModel.CancelEventHandler(wbUmbrellaExplorer_NewWindow);
            _view.WbUmbrellaExplorer.Navigated += new WebBrowserNavigatedEventHandler(wbUmbrellaExplorer_Navigated);
            _view.WbUmbrellaExplorer.Url = new Uri(ScraperBanescoResources.URLLogin);
            _view.WbUmbrellaExplorer.ScriptErrorsSuppressed = true;
        }
        #endregion

        #region Events
        private void Browser_Load(object sender, EventArgs e)
        {
            LoadCustomFont();
            _view.TsbHome.Font = new Font(private_fonts.Families[0], 12);
            _view.TsbLogin.Font = new Font(private_fonts.Families[0], 12);
            _view.TsddbMenu.Font = new Font(private_fonts.Families[0], 10);
            _view.TsmiDates.Font = new Font(private_fonts.Families[0], 12);
            _view.TsmiAccount.Font = new Font(private_fonts.Families[0], 12);
            _view.TsmiLogout.Font = new Font(private_fonts.Families[0], 12);
            _view.TslUser.Font = new Font(private_fonts.Families[0], 13);
            _view.TslPassword.Font = new Font(private_fonts.Families[0], 13);
            _view.TsddbMenu.DropDownDirection = ToolStripDropDownDirection.BelowLeft;
            _view.TstbPassword.TextBox.UseSystemPasswordChar = true;
            _view.TstbPassword.Font = new Font(private_fonts.Families[0], 9);
            _view.WbUmbrellaExplorer.Url = new Uri(ScraperBanescoResources.URLLogin);
            _view.WbUmbrellaExplorer.Navigate(new Uri(ScraperBanescoResources.URLLogin));
            FillEntryValues(_umbrellaUser.IdUSocialReason, ScraperResources.BankOptionBanesco, ScraperResources.FileExtensionCsv);
            bankId = Bank.Banesco;
        }

        private void wbUmbrellaExplorer_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                //Actualizar la dirección web de la barra de direcciones.
                UpdateAddressBar();

                if (GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLLogin.ToLower())
                {
                    if (_count < 1)
                    {
                        Login(_umbrellaUser);
                        _count++;
                    }
                }
                //Navegar automaticamente a la pagina de cuentas y movimientos.
                else if (GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLDefault.ToLower())
                {
                    DisableUserInput();
                    NavigateWebPage();
                }
                else if (GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLAccount.ToLower())
                {
                    //Click a exportar.
                    Exportar();
                }
                else if (GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLExportar.ToLower())
                {
                    //Levantamos fiddler
                    StartFiddler();
                    //Descargar el excel con el EDC.
                    DownloadFile();
                }
                //Cierra la aplicacion al cerrar sesion
                else if (GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLSalir.ToLower())
                {
                    Application.ExitThread();
                }
                else if (GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLError.ToLower() || GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLError2.ToLower())
                {
                    throw new UserLoginException();
                }
                else if (GetCurrentUrlString().ToLower() == ScraperBanescoResources.URLLoginAlternate.ToLower())
                {
                    Application.ExitThread();
                }
                else if (GetCurrentUrlString() == "about:blank")
                {
                    _view.WbUmbrellaExplorer.Navigate(ScraperBanescoResources.URLLogin);
                }
            }
            catch (UserLoginException ex)
            {
                string[] error = GetError();
                string errorMessage = error[0];
                string errorCode = error[1];

                SetUserStatus(errorCode);

                SException exe = new SException
                    (
                        ScraperBanescoErrors.InvalidUserPWExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        errorMessage,
                        ex
                    );
                ITLogic.Log.Logger.WriteErrorLog(exe.MessageException, MethodBase.GetCurrentMethod().DeclaringType.Name);

                Application.ExitThread();
            }
            catch (Exception ex)
            {
                SException exe = new SException
                    (
                       ScraperErrors.SExceptionCode,
                       this.GetType().Name,
                       MethodBase.GetCurrentMethod().Name,
                       ScraperErrors.SExceptionMessage,
                       ex
                    );
                ITLogic.Log.Logger.WriteErrorLog(ex, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        private void wbUmbrellaExplorer_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            try
            {
                if (GetCurrentUrlString() == ScraperBanescoResources.URLLoginAlternate)
                {
                    throw new AlternateLoginException(ScraperBanescoErrors.AlternateLoginExceptionMessage);
                }
            }
            catch (AlternateLoginException ex)
            {
                SException exe = new SException
                    (
                       ScraperBanescoErrors.AlternateLoginExceptionCode,
                       this.GetType().Name,
                       MethodBase.GetCurrentMethod().Name,
                       ScraperBanescoErrors.AlternateLoginExceptionMessage,
                       ex
                    );
                ITLogic.Log.Logger.WriteWarningLog(ex.MessageException, MethodBase.GetCurrentMethod().DeclaringType.Name);

                Command commandChangeUserStatus;
                commandChangeUserStatus = CommandFactory.GetCommandChangeUserStatus(_umbrellaUser);
                commandChangeUserStatus.Parameter = new Dictionary<string, object>
                                {
                                    { GeneralResources.DictionaryKeyUserStatus, UmbrellaUserStatus.Active}
                                };
                commandChangeUserStatus.Execute();


                Application.ExitThread();
            }
            catch (Exception ex)
            {
                SException exe = new SException
                    (
                       ScraperErrors.SExceptionCode,
                       this.GetType().Name,
                       MethodBase.GetCurrentMethod().Name,
                       ScraperErrors.SExceptionMessage,
                       ex
                    );

                ITLogic.Log.Logger.WriteErrorLog(ex, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        private void tsmiLogout_Click(object sender, EventArgs e)
        {
            _closeApp = true;
            Logout(_umbrellaUser);
        }

        private void tsbLogin_Click(object sender, EventArgs e)
        {

        }

        private void tsbHome_Click(object sender, EventArgs e)
        {
            //GoHome();
        }

        private void wbUmbrellaExplorer_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseTimeoutPopUp(e);
        }

        public override void FiddlerApplication_BeforeRequest(Session oSession)
        {
            if (oSession.RequestMethod == ScraperResources.RequestMethodPost && oSession.fullUrl == ScraperBanescoResources.URLExportar)
            {
                //Logout(_umbrellaUser);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cambia el <see cref="UUserStatus"/> del <see cref="UUser"/> segun el error
        /// especifico arrojado por la pagina de banesco
        /// </summary>
        /// <param name="errorCode">codigo de error arrojado por la pagina de banesco</param>
        private void SetUserStatus(string errorCode)
        {
            Command commandChangeUserStatus;

            commandChangeUserStatus = CommandFactory.GetCommandChangeUserStatus(_umbrellaUser);

            //Todo: Pasar esto a un metodo

            switch (errorCode)
            {
                case BanescOnlineErrorConstant.UserBlocked:
                    {
                        commandChangeUserStatus.Parameter = new Dictionary<string, object>
                                {
                                    { GeneralResources.DictionaryKeyUserStatus, UmbrellaUserStatus.Blocked}
                                };
                        break;
                    }
                case BanescOnlineErrorConstant.UserJustBlocked:
                    {
                        commandChangeUserStatus.Parameter = new Dictionary<string, object>
                                {
                                    { GeneralResources.DictionaryKeyUserStatus, UmbrellaUserStatus.Blocked}
                                };
                        break;
                    }
                case BanescOnlineErrorConstant.UserPasswordExpired:
                    {
                        commandChangeUserStatus.Parameter = new Dictionary<string, object>
                                {
                                    { GeneralResources.DictionaryKeyUserStatus, UmbrellaUserStatus.PasswordExpired}
                                };
                        break;
                    }
                case BanescOnlineErrorConstant.UserInUse:
                    {
                        commandChangeUserStatus.Parameter = new Dictionary<string, object>
                                {
                                    { GeneralResources.DictionaryKeyUserStatus, UmbrellaUserStatus.InUse}
                                };
                        break;
                    }
                default:
                    commandChangeUserStatus.Parameter = new Dictionary<string, object>
                                {
                                    { GeneralResources.DictionaryKeyUserStatus, UmbrellaUserStatus.Active}
                                };
                    break;
            }
            commandChangeUserStatus.Execute();
        }

        /// <summary>
        /// Obtiene el mensaje de error de la plataforma en linea
        /// </summary>
        /// <returns>Mensaje de error de la plataforma en linea</returns>
        public string[] GetError()
        {
            string[] error = new string[2];
            try
            {
                error[0] = _view.WbUmbrellaExplorer.Document.GetElementById(ScraperBanescoResources.HtmlIDErrorMessage).InnerText;
                error[1] = _view.WbUmbrellaExplorer.Document.GetElementById(ScraperBanescoResources.HtmlIDErrorCode).InnerText;
            }
            catch (NullReferenceException e)
            {
                ITLogic.Log.Logger.WriteWarningLog(e.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return error;
        }

        /// <summary>
        /// Cierra el popup de timeout y cierra sesion
        /// </summary>
        /// <param name="e">Evento NewWindow</param>
        public void CloseTimeoutPopUp(System.ComponentModel.CancelEventArgs e)
        {
            //Suprimo el popup de errores de scripts
            _view.WbUmbrellaExplorer.ScriptErrorsSuppressed = true;
            //Cancelo el evento de NewWindow
            e.Cancel = true;
            //Cierro sesion y habilito los componentes del browser
            //GoHome();
        }

        /// <summary>
        /// Navega la pagina web para realizar la descarga del EDC
        /// </summary>
        public void DownloadFile()
        {
            //Click a la opcion de configurar parametros.
            _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperBanescoResources.HtmlIDParametros)
                .InvokeMember(ScraperResources.HtmlMemberClick);
            //Click a la opcion de encabezados.
            _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperBanescoResources.HtmlIDEncabezadoSi)
                .InvokeMember(ScraperResources.HtmlMemberClick);
            //Click a la opcion de division de campos por delimitador.
            _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperBanescoResources.HtmlIDDelimitador)
                .InvokeMember(ScraperResources.HtmlMemberClick);
            //Obtengo las opciones del select de delimitadores.
            HtmlElement select = _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperBanescoResources.HtmlIDDelimitadores).Children[2];
            //Coloco como selected a la opcion del delimitador que quiero.
            select.SetAttribute(ScraperResources.HtmlAttributeSelected, ScraperResources.HtmlAttributeSelected);
            //Levanto el evento de onChange del select de delimitadores.
            select.InvokeMember(ScraperResources.HtmlOnChangeEvent);
            //Click al boton de aceptar la exportacion
            _view.WbUmbrellaExplorer.Document.GetElementById(ScraperBanescoResources.HtmlIDAceptarExportar)
                .InvokeMember(ScraperResources.HtmlMemberClick);
        }

        /// <summary>
        ///Busca el boton de exportar y invoca el "click" en el boton
        /// </summary>
        public void Exportar()
        {
            _view.WbUmbrellaExplorer.Document.GetElementById(ScraperResources.HtmlIDContentRight)
                .GetElementsByTagName(ScraperResources.HtmlTagInput)
                .GetElementsByName(ScraperBanescoResources.HtmlNameInput)[0]
                .InvokeMember(ScraperResources.HtmlMemberClick);
        }

        /// <summary>
        /// Realiza una acción de navegación a la dirección web especificada
        /// </summary>
        public void NavigateWebPage()
        {
            _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperResources.HtmlIDContentRight).Children[4]
                .GetElementsByTagName(ScraperResources.HtmlTagLink)[0]
                .InvokeMember(ScraperResources.HtmlMemberClick);
        }

        /// <summary>
        /// Seleccion de visualizacion por rango del EDC. Ingresa parametros 
        /// del rango de fechas en la pagina web.
        /// </summary>
        /// <param name="startDate">Fecha de inicio del EDC</param>
        /// <param name="endDate">Fecha fin del EDC</param>
        public void SelectRange(string startDate, string endDate)
        {
            //Seleccionar la opcion de ver EDC por rango
            _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperBanescoResources.HtmlIDRadioRango)
                .InvokeMember(ScraperResources.HtmlMemberClick);

            _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperBanescoResources.HtmlIDFechaDesde)
                .SetAttribute(ScraperResources.HtmlAttributeValue, startDate);

            _view.WbUmbrellaExplorer.Document
                .GetElementById(ScraperBanescoResources.HtmlIDFechaHasta)
                .SetAttribute(ScraperResources.HtmlAttributeValue, endDate);
        }

        /// <summary>
        /// Cierra sesión si ya existe una sesion abierta. Cierra la aplicacion por defecto.
        /// </summary>
        //public void Logout(bool closeApp)
        //{
        //    //Cierra la aplicacion si no se ha iniciado sesion.
        //    if (_view.WbUmbrellaExplorer.Url.ToString() == ScraperBanescoResources.URLLogin && closeApp == true)
        //    {
        //        Application.Exit();
        //    }
        //    //Cierra sesion si existe una abierta, de lo contrario cierra la aplicacion.
        //    else
        //    {
        //        try
        //        {
        //            _view.WbUmbrellaExplorer.Document
        //                .GetElementById(ScraperBanescoResources.HtmlIDSalir)
        //                .InvokeMember(ScraperResources.HtmlMemberClick);
        //        }
        //        catch (Exception)
        //        {
        //            if (closeApp == true)
        //            {
        //                Application.Exit();
        //            }
        //            else if (closeApp == false)
        //            {
        //                Navigate(ScraperBanescoResources.URLSalir);
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// Llena los TextBox de las credenciales si fueron guardadas previamente en el App.config
        /// </summary>
        public void FillCredentials()
        {
            if (_view.TsmiAccountBFI.Checked)
            {
                _view.TstbUser.Text = ITSecurity.RetrieveSecureCredentials(AccountField.UserBFI.ToString());
                _view.TstbPassword.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PWBFI.ToString());
            }
            else if (_view.TsmiAccountTD.Checked)
            {
                _view.TstbUser.Text = ITSecurity.RetrieveSecureCredentials(AccountField.UserTD.ToString());
                _view.TstbPassword.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PWTD.ToString());
            }
        }

        #endregion

        #region Obsolete

        [Obsolete("Se eliminan las funciones de la interfaz")]
        /// <summary>
        /// Cierra la sesion del usuario del banco y retorna a la pagina de login
        /// </summary>
        public void GoHome()
        {
            if (_view.WbUmbrellaExplorer.Url.ToString() != ScraperBanescoResources.URLLogin)
            {
                Logout(_umbrellaUser);
            }
        }

        #endregion
    }
}
