using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.Threading;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Security;
using CefSharp.Example;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITExceptions.Scraper;
using BLL.Concrete;
using InstaTransfer.ITResources.Scraper.ScraperBanesco2;
using InstaTransfer.ITResources.Constants;
using System.Reflection;
using System.Security.Cryptography;
using InstaTransfer.ITResources.Enums;
using System.Timers;
using InstaTransfer.ITExceptions.Scraper.Banesco;
using InstaTransfer.ITResources.Scraper;
using ITCSScraper;

namespace ITCSScraper
{
    public partial class SBrowserProvincial : Form
    {
        #region Variables
        private readonly ChromiumWebBrowser browser;

        delegate void WriteErrorDelegate(string[] error);

        private int count;
        private int count2;
        private int count3;
        private int count4;
        public string username;
        public string password;
        public string rif;
        string[] error = new string[2];
        bool loggedIn;
        bool accountVerified;

        public System.Timers.Timer Timeout = new System.Timers.Timer();

        public UUserBLL UUBLL = new UUserBLL();

        private UUser _currentUser;
        public UUser CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    return new UUser();
                }
                else
                {
                    return _currentUser;
                }

            }
            set { _currentUser = value; }
        }


        //private List<BankStatementRequest> _requestGroup;
        //public List<BankStatementRequest> RequestGroup
        //{
        //    get
        //    {
        //        if (_requestGroup == null)
        //        {
        //            return new List<BankStatementRequest>();
        //        }
        //        else
        //        {
        //            return _requestGroup;
        //        }

        //    }
        //    set { _requestGroup = value; }
        //}
        #endregion

        #region Constructor
        public SBrowserProvincial(UUser currentUser)
        {
            // Variables
            count = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;
            //RequestGroup = requestGroup;
            CurrentUser = currentUser;

            InitializeComponent();
            // Create a browser component
            //browser = new ChromiumWebBrowser("https://ve1.provinet.net/nhvp_ve_web/atpn_es_web_jsp/login.jsp")
            //{
            //    Dock = DockStyle.Fill
            //};

            browser = new ChromiumWebBrowser("https://ve1.provinet.net/nhvp_ve_web/atpn_es_web_jsp/login.jsp?it=2")
            {
                Dock = DockStyle.Fill
            };

            // Add it to the form and fill it to the form window.
            this.Controls.Add(browser);

            // Timer
            Timeout.Elapsed += new ElapsedEventHandler(OnTimeout);
            double time;
            //Tomo el tiempo desde el app.config. Por defecto en 3m.
            try
            {
                time = int.Parse(GeneralHelper.GetAppSettingValue("ScraperTimeout"));
            }
            catch (Exception)
            {
                time = 180000;
            }
            Timeout.Interval = time;
            Timeout.Enabled = true;

            //Handlers
            browser.DownloadHandler = new DownloadHandler(CurrentUser);
            browser.JsDialogHandler = new JsDialogHandler();

            //Events
            browser.LoadingStateChanged += OnLoadingStateChanged;


            // Context
            RequestContextSettings requestContextSettings = new RequestContextSettings();

            requestContextSettings.PersistSessionCookies = false;
            requestContextSettings.PersistUserPreferences = false;

            browser.RequestContext = new RequestContext(requestContextSettings);
        }
        #endregion

        #region Events
        private void SBrowserBanesco_FormClosing(object sender, FormClosingEventArgs e)
        {
            Exit();
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            try
            {
                if (browser.IsBrowserInitialized && !browser.IsDisposed && !browser.Disposing)
                {
                    if ((browser.GetMainFrame().Url == "https://ve1.provinet.net/nhvp_ve_web/atpn_es_web_jsp/login.jsp" ||
                        browser.GetMainFrame().Url == "https://ve1.provinet.net/nhvp_ve_web/atpn_es_web_jsp/login.jsp?it=2")/* && !args.IsLoading*/)
                    {
                        // Iniciamos sesion en la plataforma
                        Login();
                    }
                    else if (browser.GetMainFrame().Url == "https://ve1.provinet.net/shvp_ve_web/atpn_es_web_jsp/portal.jsp" && !args.IsLoading)
                    {
                        // Consultamos la cuenta bancaria especifica
                        NavigatePage();
                    }
                    else if (browser.GetMainFrame().Url == "https://ve1.provinet.net/shvp_ve_web/atpn_es_web_jsp/desconectar.jsp")
                    {
                        // Cerramos la aplicacion
                        Exit();
                    }
                    else if (browser.GetMainFrame().Url == "about:blank")
                    {
                        // Cerramos la aplicacion
                        Exit();
                    }
                }
            }
            catch (InvalidBankAccountException e)
            {
                // Cerramos la aplicacion
                Exit();
            }
            catch (CryptographicException)
            {
                // Cambiamos el estado de la solicitud a error en decriptacion
                using (UUserBLL UUBLL = new UUserBLL())
                {
                    UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.DecryptError);
                }

                // Cerramos la aplicacion
                Exit();
            }
            catch (UserLoginException e)
            {
                bool result;
                // Guardamos los errores en BD
                using (UUserBLL UUBLL = new UUserBLL())
                {
                    result = UUBLL.WriteError(CurrentUser, e.MessageException, e.ErrorCode);
                }

                // Cerramos la aplicacion
                Exit();
            }
            catch (Exception)
            {
                // Cerramos la aplicacion
                Exit();
            }
        }

        public void OnTimeout(object source, ElapsedEventArgs e)
        {
            //Exit();

        }
        #endregion

        #region Methods

        /// <summary>
        /// Inicia sesion en la plataforma bancaria
        /// </summary>
        public void Login()
        {
            if (count < 1)
            {
                // Obtenemos el usuario y contraseña desde la solicitud actual
                username = ITSecurity.DecryptUserCredentials(CurrentUser.Username);
                password = ITSecurity.DecryptUserCredentials(CurrentUser.Password);
                rif = CurrentUser.IdUSocialReason.Remove(0, 1);

                // Suprimimos los alerts de JS
                //browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_frmAplicacion').contentWindow.alert = function() {};");

                // Ingresamos el usuario
                browser.ExecuteScriptAsync(String.Format("document.getElementById('COD_USU').value = '{0}'", username));
                browser.ExecuteScriptAsync("document.getElementById('COD_USU').onchange()");
                // Ingresamos la contraseña
                browser.ExecuteScriptAsync(String.Format("document.getElementById('cod_cvepass').value = '{0}'", password));
                browser.ExecuteScriptAsync("document.getElementById('cod_cvepass').onchange()");
                // Tipo de documento
                browser.ExecuteScriptAsync("document.getElementById('DOCTARJ').children[1].setAttribute('selected', 'selected')");
                browser.ExecuteScriptAsync("document.getElementById('DOCTARJ').onchange()");
                // Numero de rif
                browser.ExecuteScriptAsync(String.Format("document.getElementById('NUMDOCR').value = '{0}'", rif.Substring(0, 8)));
                browser.ExecuteScriptAsync("document.getElementById('NUMDOCR').onchange()");
                // Digito de verificacion del rif
                browser.ExecuteScriptAsync(String.Format("document.getElementById('NUMIDER').value = '{0}'", rif[8].ToString()));
                browser.ExecuteScriptAsync("document.getElementById('NUMIDER').onchange()");
                // Entrar
                browser.ExecuteScriptAsync("document.getElementById('ingresarLogin').click()");
                //// Verificamos si existe un error
                //GetError();
                //// Verificamos si existen errores
                //if (error[0] != null)
                //{
                //    throw new UserLoginException(error[0], error[1]);
                //}
                count++;
            }

        }

        public void NavigatePage()
        {

            // Buscamos si existe el enlace a las cuentas
            browser.EvaluateScriptAsync("window.frames['Fprincipal'].document.getElementsByClassName('Link1')[0].innerText").ContinueWith(t =>
            {
                if (t.Result.Success && t.Result.Result != null)
                {
                    // Intentamos seleccionar la cuenta
                    if (t.Result.Result.ToString().Replace("-", "") == UUBLL.GetBankAccount(CurrentUser, "0108"))
                    {
                        if (count2 < 1)
                        {
                            browser.ExecuteScriptAsync("window.frames['Fprincipal'].document.getElementsByClassName('Link1')[0].click()");
                            count2++;
                        }
                    }
                    else
                    {
                        if (count3 < 1)
                        {
                            browser.ExecuteScriptAsync("window.frames['Fprincipal'].document.getElementsByClassName('Link1')[0].click()");
                            count3++;
                        }
                    }

                }
            });

            // Buscamos si existe el boton descargar
            browser.EvaluateScriptAsync("window.frames['Fprincipal'].document.getElementsByName('btnDescargar')[0].value").ContinueWith(t =>
            {
                if (t.Result.Success && t.Result.Result != null)
                {
                    if (count4 < 1)
                    {
                        // Descargamos el archivo del dia
                        browser.ExecuteScriptAsync("window.frames['Fprincipal'].document.getElementsByName('btnDescargar')[0].click()");
                        count4++;
                    }

                }
            });
        }

        /// <summary>
        /// Consulta la cuenta bancaria
        /// </summary>
        public void ConsultAccount()
        {
            // Variables
            accountVerified = false;
            loggedIn = true;
            Timeout.AutoReset = true;
            string lastDigits;

            // Cambio el estado a en uso
            using (UUserBLL UUBLL = new UUserBLL())
            {
                UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.InUse);
            }

            // Pasamos el usuario actual al DownloadHandler
            ((DownloadHandler)browser.DownloadHandler).CurrentUser = CurrentUser;

            // Obtenemos los ultimos digitos
            using (UUserBLL UUBLL = new UUserBLL())
            {
                lastDigits = UUBLL.GetBankAccount(CurrentUser, "0108").Right(7);
            }

            // Recorremos la lista de cuentas y buscamos si existe la especifica
            browser.GetMainFrame().ExecuteJavaScriptAsync("var table = document.getElementsByClassName('GridViewHm')[0]");
            browser.GetMainFrame().ExecuteJavaScriptAsync("var accounts = table.getElementsByTagName('a')");
            browser.GetMainFrame().ExecuteJavaScriptAsync("function validateAccount(lastDigits){for (var i = 0; i < accounts.length; i++) {var accountLastDigits = accounts[i].innerHTML.slice(-7); if (accountLastDigits === lastDigits) {return true;}}}");

            //Execute extension method
            browser.GetMainFrame().EvaluateScriptAsync("validateAccount('" + lastDigits + "')").ContinueWith(task =>
            {
                // Now we're not on the main thread, perhaps the
                // Cef UI thread. It's not safe to work with
                // form UI controls or to block this thread.
                // Queue up a delegate to be executed on the
                // main thread.
                BeginInvoke(new Action(() =>
            {
                string message;
                if (task.Exception == null)
                {
                    message = task.Result.ToString();
                }
                else
                {
                    message = string.Format("Script evaluation failed. {0}", task.Exception.Message);
                }
            // Evaluo el resultado de la respuesta
            if (task.Result.Success && task.Result != null)
                {


                    if (!accountVerified && task.Result.Result == null)
                    {
                        count3 = 0;

                    // Invalidamos la cuenta
                    using (UUserBLL UUBLL = new UUserBLL())
                        {
                            UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.InvalidBankAccount);
                        }
                        browser.GetMainFrame().LoadUrl("https://www.banesconline.com/mantis/WebSite/default.aspx");
                    //throw new InvalidBankAccountException(ScraperErrors.InvalidBankAccountExceptionCode, ScraperErrors.InvalidBankAccountExceptionMessage);
                }
                    else
                    {
                    // Verificamos el resultado
                    Boolean.TryParse(task.Result.Result.ToString(), out accountVerified);
                    }

                }

            }));
            });

            // Seleccionamos la cuenta bancaria
            browser.GetMainFrame().ExecuteJavaScriptAsync("for (var i = 0; i < accounts.length; i++) {var accountLastDigits = accounts[i].innerHTML.slice(-7); if (accountLastDigits === '" + lastDigits + "') {accounts[i].click();}};");

            count2 = 0;

            // Verificamos si existe un error
            GetError();
        }

        /// <summary>
        /// Exportamos los movimientos con los parametros especificos
        /// </summary>
        public void Export()
        {
            // Opcion de movimientos por rango
            browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_rdbRango').click()");
            // Fecha inicio
            browser.GetMainFrame().ExecuteJavaScriptAsync(string.Format("document.getElementById('ctl00_cp_dtFechaDesde').value = '{0}'", DateTime.Now.ToString("dd/MM/yyyy")));
            // Fecha fin
            browser.GetMainFrame().ExecuteJavaScriptAsync(string.Format("document.getElementById('ctl00_cp_dtFechaHasta').value = '{0}'", DateTime.Now.ToString("dd/MM/yyyy")));
            // Boton exportar
            browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementsByName('ctl00$cp$ctl25')[0].click()");
        }

        /// <summary>
        /// Navega la pagina web para realizar la descarga del EDC
        /// </summary>
        public void DownloadFile()
        {
            if (count2 < 1)
            {
                browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_optParametros').click()");
                browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_optEncabezadoSi').click()");
                browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_optDelimitador').click()");
                browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_ddlDelimitadores').children[2].setAttribute('selected','selected')");
                browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_btnOk').click()");
            }
            count3 = 0;
            count2++;
        }

        /// <summary>
        /// Cierra la aplicacion en el hilo actual
        /// </summary>
        public void Exit()
        {
            this.Invoke((MethodInvoker)delegate
            {
                // close the form on the forms thread
                Application.ExitThread();
            });
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Obtiene el mensaje de error de la plataforma en linea
        /// </summary>
        /// <returns>Mensaje de error de la plataforma en linea</returns>
        public string[] GetError()
        {
            // Variables
            WriteErrorDelegate _writeError;
            _writeError = WriteError;

            try
            {

                // Obtengo el mensaje de error desde la pagina
                browser.GetMainFrame().EvaluateScriptAsync("document.getElementById('ctl00_cp_frmAplicacion').contentDocument.getElementById('lblMensaje').innerText").ContinueWith(r =>
                {
                    // Guardo la respuesta
                    var response = r.Result;

                    // Evaluo el resultado de la respuesta
                    if (response.Success && response.Result != null)
                    {
                        // Guardo el resultado
                        error[0] = response.Result.ToString();
                    }
                    // Guardamos el error
                    _writeError(error);
                });

                // Obtengo el codigo de error desde la pagina
                browser.GetMainFrame().EvaluateScriptAsync("document.getElementById('ctl00_cp_frmAplicacion').contentDocument.getElementById('lblCodigoError').innerText").ContinueWith(r =>
                {
                    // Guardo la respuesta
                    var response = r.Result;

                    // Evaluo el resultado de la respuesta
                    if (response.Success && response.Result != null)
                    {
                        // Guardo el resultado
                        error[1] = response.Result.ToString();
                    }
                    if (error[0] != null)
                    {
                        // Evaluo el mensaje de error y asigno el codigo correspondiente
                        if (error[0].Contains(ScraperBanesco2Errors.InvalidPasswordErrorMessage) || error[0].Contains(ScraperBanesco2Errors.InvalidUserPWExceptionMessage))
                        {
                            error[1] = BanescOnlineErrorConstant.UserPasswordInvalid;
                        }
                        else if (error[0].Contains(ScraperBanesco2Errors.UserBlockedErrorMessage))
                        {
                            error[1] = BanescOnlineErrorConstant.UserJustBlocked;
                        }
                        else
                        {
                            error[1] = response.Result != null ? response.Result.ToString() : BanescOnlineErrorConstant.GeneralError;
                        }

                        // Guardamos el error
                        _writeError(error);
                    }
                });

                // Obtengo el mensaje de error desde la pagina
                browser.GetMainFrame().EvaluateScriptAsync("document.getElementsByClassName('InfoDiv')[0].innerText").ContinueWith(r =>
                {
                    // Guardo la respuesta
                    var response = r.Result;

                    // Evaluo el resultado de la respuesta
                    if (response.Success && response.Result != null)
                    {
                        // Guardo el resultado
                        error[0] = response.Result.ToString();
                    }
                    // Guardamos el error
                    _writeError(error);
                });

            }
            catch (ArgumentException e)
            {
                error[0] = null;
            }
            catch (NullReferenceException e)
            {
                InstaTransfer.ITLogic.Log.Logger.WriteWarningLog(e.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                InstaTransfer.ITLogic.Log.Logger.WriteWarningLog(e.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return error;
        }

        /// <summary>
        /// Guarda el error
        /// </summary>
        /// <param name="_error"></param>
        public void WriteError(string[] _error)
        {
            // Variables
            bool result;

            // Verificamos si existe el error
            if (error[0] != null)
            {
                error[0] = _error[0];
            }
            if (error[1] != null)
            {
                error[1] = _error[1];
            }
            if (error[0] != null && error[1] != null)
            {
                // Guardamos los errores en BD
                using (UUserBLL UUBLL = new UUserBLL())
                {
                    result = UUBLL.WriteError(CurrentUser, error[0], error[1]);
                }

                // Cerramos la aplicacion
                Exit();

            }


        }

        public async Task<string> GetSource(ChromiumWebBrowser _browser)
        {
            var source = await _browser.GetMainFrame().GetSourceAsync();
            return source;
        }

        #endregion
    }
}

