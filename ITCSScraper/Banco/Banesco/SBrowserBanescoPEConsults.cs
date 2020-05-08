using BLL.Concrete;
using CefSharp;
using CefSharp.Example;
using CefSharp.WinForms;
using InstaTransfer.BLL.Models.Email;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Scraper;
using InstaTransfer.ITExceptions.Scraper.Banesco;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Scraper.ScraperBanesco2;
using InstaTransfer.ITResources.Scraper.ScraperBanescoPE;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ITCSScraper
{
    public partial class SBrowserBanescoPEConsults : Form
    {
        #region Variables
        private readonly ChromiumWebBrowser browser;

        delegate void WriteErrorDelegate(string[] error);

        private int loginCount;
        private int count2;
        private int count3;
        private int smsRequestCount;
        private int searchCount;

        public string username;
        public string password;
        string[] error = new string[2];
        bool loggedIn;
        bool accountVerified;

        public System.Timers.Timer Timeout = new System.Timers.Timer();

        public UUserBLL UUBLL = new UUserBLL();
        public AE_ArchivoBLL AEABLL = new AE_ArchivoBLL();

        public List<AE_Archivo> ChargeAccounts = new List<AE_Archivo>();


        private AE_Archivo _currentChargeAccount;
        public AE_Archivo CurrentChargeAccount
        {
            get
            {
                if (_currentChargeAccount == null)
                {
                    return new AE_Archivo();
                }
                else
                {
                    return _currentChargeAccount;
                }

            }
            set { _currentChargeAccount = value; }
        }

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

        #endregion

        #region Constructor
        public SBrowserBanescoPEConsults(UUser currentUser)
        {
            // Variables
            loginCount = 0;
            count2 = 0;
            count3 = 0;
            smsRequestCount = 0;
            searchCount = 0;
            //RequestGroup = requestGroup;
            ChargeAccounts = AEABLL.GetFilesByStatus(new List<ChargeAccountStatus> { ChargeAccountStatus.Uploaded, ChargeAccountStatus.Accepted });
            CurrentChargeAccount = ChargeAccounts.FirstOrDefault();
            CurrentUser = currentUser;

            InitializeComponent();
            // Create a browser component
            browser = new ChromiumWebBrowser("https://pagoelectronico.banesco.com/lazaro/WebSite/login.aspx")
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

            // Handlers
            browser.DownloadHandler = new DownloadHandler(CurrentUser);
            browser.JsDialogHandler = new JsDialogHandler();

            //Events
            browser.LoadingStateChanged += OnLoadingStateChanged;

            // Context
            RequestContextSettings requestContextSettings = new RequestContextSettings();

            requestContextSettings.PersistSessionCookies = false;
            requestContextSettings.PersistUserPreferences = false;

            browser.RequestContext = new RequestContext(requestContextSettings);

            // Limpiamos los mensajes SMS de la cola
            CleanSmsKeys();
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
                    if (browser.GetMainFrame().Url == "https://pagoelectronico.banesco.com/lazaro/WebSite/login.aspx" && !args.IsLoading)
                    {
                        // Iniciamos sesion en la plataforma
                        Login();
                    }
                    else if (browser.GetMainFrame().Url == "https://pagoelectronico.banesco.com/lazaro/WebSite/Default.aspx" && !args.IsLoading)
                    {
                        // Navega al Gestor de Documents
                        ManageDocuments();
                    }
                    else if (browser.GetMainFrame().Url == "https://pagoelectronico.banesco.com/lazaro/WebSite/Aplicacion.aspx?opc=18" && !args.IsLoading)
                    {
                        // Revisamos la lista de documentos
                        FindDocument();
                    }
                    else if (browser.GetMainFrame().Url == "https://www.banesconline.com/mantis/WebSite/ConsultaMovimientosCuenta/Exportar.aspx" && !args.IsLoading)
                    {
                        // Descargamos el EDC con los parametros especificos
                        DownloadFile();
                    }
                    else if (browser.GetMainFrame().Url == "https://pagoelectronico.banesco.com/lazaro/WebSite/salir.aspx")
                    {
                        // Cambiamos el estado del usuario
                        CurrentUser = UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.Active);
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
                    CurrentUser = UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.DecryptError);
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
            Exit();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Inicia sesion en la plataforma bancaria
        /// </summary>
        public void Login()
        {
            // Variables
            string smsKey;
            int smsRequestLimit = Convert.ToInt32(ConfigurationManager.AppSettings["SmsRequestLimit"]);
            // Obtenemos el usuario y contraseña desde la solicitud actual
            username = ITSecurity.DecryptUserCredentials(CurrentUser.Username);
            password = ITSecurity.DecryptUserCredentials(CurrentUser.Password);

            //username = "peworker1";
            //password = "Hq$T2ENm8X7%";

            // Suprimimos los alerts de JS
            browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_frmAplicacion').contentWindow.alert = function() {};");

            // Ingresamos el usuario, si existe el campo ingresamos la contraseña y presionamos aceptar
            browser.GetBrowser().GetFrame(3).EvaluateScriptAsync(String.Format("document.getElementById('txtUsuario').value = '{0}'", username)).ContinueWith(t =>
            {
                if (t.Result.Success && loginCount < 1)
                {
                    // Ingresar clave
                    browser.GetBrowser().GetFrame(3).ExecuteJavaScriptAsync(string.Format("document.getElementById('txtClave').value = '{0}'", password));
                    // Aceptar
                    browser.GetBrowser().GetFrame(3).ExecuteJavaScriptAsync("document.getElementById('bAceptar').click()");
                    loginCount++;
                }
            });

            // Ingresamos la clave de sms
            browser.GetBrowser().GetFrame(3).EvaluateScriptAsync(String.Format("document.getElementById('Label1').innerText")).ContinueWith(t =>
            {
                if (t.Result.Success && loginCount < 2)
                {
                    // Esperamos 5s para que llegue el mensaje
                    Thread.Sleep(5000);
                    // Llamamos al api hasta que obtengamos respuesta. Si no hay respuesta volvemos a iniciar sesion.
                    while (string.IsNullOrEmpty(smsKey = GetSmsKey()) && smsRequestCount < smsRequestLimit) { smsRequestCount++; };

                    // Verificamos si obtuvimos la clave
                    if (!string.IsNullOrEmpty(smsKey))
                    {
                        // Ingresamos la clave de operaciones especiales
                        browser.GetBrowser().GetFrame(3).ExecuteJavaScriptAsync(string.Format("document.getElementById('txtClave').value = '{0}'", smsKey));
                        // Presionamos aceptar
                        browser.GetBrowser().GetFrame(3).ExecuteJavaScriptAsync("document.getElementById('btnAceptar').click()");
                        loginCount++;
                    }
                    else
                    {
                        Logger.WriteErrorLog("Error al pedir el codigo sms", "SBrowserBanescoPEConsults (Login)");

                        // Envio correo de fallo al leer sms
                        SendMail(EmailType.SmsFailure);

                        // Echo pa' tras
                        browser.GetBrowser().GoBack();

                        // Reseteo el contador
                        loginCount = 0;
                    }
                }
            });


            // Verificamos si existe un error
            GetError();
            // Verificamos si existen errores
            if (error[0] != null)
            {
                throw new UserLoginException(error[0], error[1]);
            }
        }

        /// <summary>
        /// Navega a la pagina de gestor de documetos
        /// </summary>
        public void ManageDocuments()
        {
            // Cambiamos el estado del usuario
            CurrentUser = UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.InUse);

            // Cargamos la pagina del gestor de documentos en el navegador
            browser.GetMainFrame().LoadUrl("https://pagoelectronico.banesco.com/lazaro/WebSite/Aplicacion.aspx?opc=18");
        }

        /// <summary>
        /// Buscamos el documento especifico
        /// </summary>
        public void FindDocument()
        {
            /// Contamos hasta 2 porque la pagina carga dos veces antes de mostrar la lista de documentos.
            /// Adicionalmente carga cada vez que se presiona "Buscar"
            if (searchCount < 2)
            {
                // Opcion de documentos por rango
                browser.GetMainFrame().ExecuteJavaScriptAsync("window.frames['ctl00_cp_frmAplicacion'].contentDocument.getElementById('rfFechas_rdoRango').click()");

                // Fecha inicio
                browser.GetMainFrame().ExecuteJavaScriptAsync(string.Format("window.frames['ctl00_cp_frmAplicacion'].contentDocument.getElementById('rfFechas_dtFechaDesde').value = '{0}'", DateTime.Now.AddDays(-5).ToString("dd/MM/yyyy")));

                // Ingresamos el código de la operación en el filtro
                browser.GetMainFrame().ExecuteJavaScriptAsync(string.Format("window.frames['ctl00_cp_frmAplicacion'].contentDocument.getElementById('textBoxNumeroDocumento').value = '{0}'", CurrentChargeAccount.Valores));

                // Seleccionamos "Buscar"
                browser.GetMainFrame().ExecuteJavaScriptAsync("window.frames['ctl00_cp_frmAplicacion'].contentDocument.getElementById('btnBuscar').click()");

                // Incrementamos el contador de busquedas
                searchCount++;
            }
            // Revisamos la respuesta unicamente al finalizar la busqueda
            else if (searchCount == 2)
            {
                // Revisamos la respuesta
                browser.GetMainFrame().EvaluateScriptAsync("window.frames['ctl00_cp_frmAplicacion'].contentDocument.getElementById('gridViewBusqueda_ctl02_Td3').innerText").ContinueWith(t =>
                {
                        // Escribimos el ressultado de la respuesta segun sea el caso
                        if (t.Result.Success)
                    {
                        if ((string)t.Result.Result == ScraperBanescoPEConstant.DocumentRejected)
                        {
                                // Cambiamos el estado en la BD
                                AEABLL.ChangeFileStatus(CurrentChargeAccount, ChargeAccountStatus.Rejected);
                        }
                        else if ((string)t.Result.Result == ScraperBanescoPEConstant.DocumentAccepted)
                        {
                                // Cambiamos el estado en la BD
                                AEABLL.ChangeFileStatus(CurrentChargeAccount, ChargeAccountStatus.Accepted);
                        }
                        else if ((string)t.Result.Result == ScraperBanescoPEConstant.CreditRegistered)
                        {
                                // Cambiamos el estado en la BD
                                AEABLL.ChangeFileStatus(CurrentChargeAccount, ChargeAccountStatus.Registered);
                        }
                        Thread.Sleep(5000);
                            // Cerramos sesion
                            browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_TopButtonHome_lnkSalir').click()");
                    }
                });
            }


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
                CurrentUser = UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.InUse);
            }

            // Pasamos el usuario actual al DownloadHandler
            ((DownloadHandler)browser.DownloadHandler).CurrentUser = CurrentUser;

            // Obtenemos los ultimos digitos
            using (UUserBLL UUBLL = new UUserBLL())
            {
                lastDigits = UUBLL.GetBankAccount(CurrentUser, "0134").Right(7);
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

        /// <summary>
        /// Obtiene la clave de operaciones especiales desde el api de SMS
        /// </summary>
        /// <returns>Clave de operaciones especiales</returns>
        public string GetSmsKey()
        {
            // Variables
            string smsKey = string.Empty;

            try
            {
                // Construimos el body
                StringBuilder requestBody = new StringBuilder("");
                string query = requestBody.ToString();
                string url = ConfigurationManager.AppSettings[ScraperBanescoPEResources.AppSettingsKeyGetSmsUrl].ToString();
                byte[] queryStream = Encoding.UTF8.GetBytes(query);

                //Llamo al API con el url que contiene los parámetros
                WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = "GET";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = queryStream.Length;
                //Stream reqStream = req.GetRequestStream();
                //reqStream.Write(queryStream, 0, queryStream.Length);
                //reqStream.Close();

                // Response
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                GetSmsModelResponse _getSmsModelResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetSmsModelResponse)) as GetSmsModelResponse;

                // Verificamos la respuesta
                if (_getSmsModelResponse.sucess)
                {
                    // Success
                    smsKey = _getSmsModelResponse.message;
                }
                else
                {
                    // Fallida
                    if (_getSmsModelResponse.message.Contains("No se puede leer"))
                    {
                        // Mandar correo de error con el modem
                    }
                }
            }
            catch (Exception)
            {
                // Error

            }


            // Retornamos la clave
            return smsKey;
        }

        /// <summary>
        /// Limpia la cola de mensajes SMS
        /// </summary>
        public void CleanSmsKeys()
        {
            // Limpiamos los mensajes
            GetSmsKey();
        }

        /// <summary>
        /// Envia correos segun el tipo de correo a enviar
        /// </summary>
        /// <param name="type"></param>
        public bool SendMail(EmailType type)
        {
            // Variables
            EmailHelper emailHelper = new EmailHelper();
            EmailModels emailModel = new EmailModels();

            try
            {
                switch (type)
                {
                    case EmailType.NewPaymentRequest:
                        break;
                    case EmailType.DeclarationSuccess:
                        break;
                    case EmailType.ReconciliationSuccess:
                        break;
                    case EmailType.RecoverCommerceUserPW:
                        break;
                    case EmailType.RecoverEndUserPW:
                        break;
                    case EmailType.SmsFailure:
                        {
                            // Construimos el correo
                            emailModel = BuildEmailBody(type);

                            // Todo (BanescoPEConsults): Colocarle al request que tuvo error de sms
                            break;
                        }
                    default:
                        break;
                }

            }
            catch (Exception)
            {
                // Error
                return false;
                throw;
            }
            // Enviamos el correo
            emailHelper.SendEmailMessage(emailModel.From, emailModel.DisplayName, emailModel.To, emailModel.Subject, emailModel.Body);
            return true;
        }

        /// <summary>
        /// Construye el cuerpo del correo
        /// </summary>
        /// <param name="model">Modelo de datos del correo</param>
        /// <param name="type">Tipo de correo</param>
        /// <returns>Cuerpo del correo</returns>
        public EmailModels BuildEmailBody(EmailType type)
        {

            // Variables
            var messageSB = new StringBuilder();
            var emailModel = new EmailModels();

            // Construimos el correo segun el tipo
            switch (type)
            {
                case EmailType.NewPaymentRequest:
                    break;
                case EmailType.DeclarationSuccess:
                    break;
                case EmailType.ReconciliationSuccess:
                    break;
                case EmailType.RecoverCommerceUserPW:
                    break;
                case EmailType.RecoverEndUserPW:
                    break;
                case EmailType.SmsFailure:
                    {
                        // Casteamos el objeto
                        //var request = (InstaTransfer.DataAccess.PaymentRequest)model;
                        // Construimos el cuerpo del correo
                        messageSB.AppendLine("<p>No se pudo establecer la conexión con el modem de sms<strong></p>");
                        // Construimos el encabezado del correo
                        emailModel = new EmailModels.PaymentRequestEmailModel
                        {
                            From = ConfigurationManager.AppSettings[ScraperBanescoPEResources.AppSettingsKeyTransaxNoReplyMail],
                            To = ConfigurationManager.AppSettings[ScraperBanescoPEResources.AppSettingsKeySmsFailureMailTo].Split(';').ToList(),
                            Subject = "Error de conexión al modem sms",
                            DisplayName = "Soporte Transax",
                            Body = messageSB.ToString()
                        };
                        break;
                    }
                default:
                    break;
            }

            // Retornamos el modelo del correo
            return emailModel;
        }

        /// <summary>
        /// Modelo de la respuesta del metodo del api de SMS
        /// </summary>
        public class GetSmsModelResponse
        {
            /// <summary>
            /// Estado
            /// </summary>
            public bool sucess { get; set; }
            /// <summary>
            /// Mensaje de Respuesta
            /// </summary>
            public string message { get; set; }
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

