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
    public partial class SBrowserBanescoPEUploads : Form
    {
        #region Variables
        private readonly ChromiumWebBrowser browser;

        delegate void WriteErrorDelegate(string[] error);

        private int count;
        private int count2;
        private int count3;
        int smsRequestCount = 0;
        public string username;
        public string password;
        public string localPath = string.Empty;
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
        public SBrowserBanescoPEUploads(UUser currentUser)
        {
            // Variables
            count = 0;
            count2 = 0;
            count3 = 0;
            ChargeAccounts = AEABLL.GetPendingFiles();
            CurrentChargeAccount = ChargeAccounts.FirstOrDefault();
            CurrentUser = currentUser;
            
            try
            {
                // Descargamos el cargo en cuenta por ftp
                localPath = DownloadFtpFile(CurrentChargeAccount.Ruta, ConfigurationManager.AppSettings[ScraperBanescoPEResources.AppSettingsKeyBasePath]);
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, "SBrowserBanescoPEUploads (DownloadFtpFile)");
                Exit();
            }


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
            browser.DialogHandler = new UploadFileDialogHandler(localPath);

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
                        // Consultamos la cuenta bancaria especifica
                        ChargeAccount();
                    }
                    else if (browser.GetMainFrame().Url == "https://pagoelectronico.banesco.com/lazaro/WebSite/envioarchivos/envio.aspx?tipo=Cobros" && !args.IsLoading)
                    {
                        // Exportamos los movimientos
                        UploadFile();
                    }
                    else if (browser.GetMainFrame().Url == "https://www.banesconline.com/mantis/WebSite/ConsultaMovimientosCuenta/Exportar.aspx" && !args.IsLoading)
                    {
                        // Descargamos el EDC con los parametros especificos
                        DownloadFile();
                    }
                    else if (browser.GetMainFrame().Url == "https://pagoelectronico.banesco.com/lazaro/WebSite/salir.aspx")
                    {
                        // Cambiamos el estado del usuario
                        UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.Active);
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
            //// Cambiamos el estado a timeout
            //UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatusEnum.Timeout);

            //// Si el usuario esta logeado navegamos a las cuentas, de lo contrario cerramos la aplicacion
            //if (loggedIn)
            //{
            //    browser.GetMainFrame().LoadUrl("https://www.banesconline.com/mantis/WebSite/default.aspx");
            //}
            //else
            //{
            //    // Cerramos la aplicacion
            //    Exit();
            //}

            // Cerramos la aplicacion
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
                if (t.Result.Success && count < 1)
                {
                    // Ingresar clave
                    browser.GetBrowser().GetFrame(3).ExecuteJavaScriptAsync(string.Format("document.getElementById('txtClave').value = '{0}'", password));
                    // Aceptar
                    browser.GetBrowser().GetFrame(3).ExecuteJavaScriptAsync("document.getElementById('bAceptar').click()");
                    count++;
                }
            });

            // Ingresamos la clave de sms
            browser.GetBrowser().GetFrame(3).EvaluateScriptAsync(String.Format("document.getElementById('Label1').innerText")).ContinueWith(t =>
            {
                if (t.Result.Success && count < 2)
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
                        count++;
                    }
                    else
                    {
                        Logger.WriteErrorLog("Error al pedir el codigo sms", "SBrowserBanescoPEUploads (Login)");

                        // Envio correo de fallo al leer sms
                        SendMail(EmailType.SmsFailure);

                        // Echo pa' tras
                        browser.GetBrowser().GoBack();

                        // Reseteo el contador
                        count = 0;
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
        /// Navega a la pagina de envio de archivos / cobros
        /// </summary>
        public void ChargeAccount()
        {
            // Cambiamos el estado del usuario
            CurrentUser = UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.InUse);
            // Navegamos a la pagina de envio de archivos
            browser.GetMainFrame().LoadUrl("https://pagoelectronico.banesco.com/lazaro/WebSite/envioarchivos/envio.aspx?tipo=Cobros");
        }

        /// <summary>
        /// Sube el archivo del cargo en cuenta
        /// </summary>
        public void UploadFile()
        {
            // Seleccionamos el tipo de operacion
            browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_wz_EnvioEE_ddlTipoArchivo').children[2].setAttribute('selected','selected')");

            // Mostramos el elemento. Esto no funciona, pero creo que hay que hacer que funcione en algunos casos
            //browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_wz_EnvioEE_fuArchivo').scrollIntoView()");

            Logger.WriteSuccessLog("Llego justo antes de cargar el archivo", "SBrowserBanescoPEUploads (UploadFile)");

            // Obtenemos las coordenadas del elemento
            browser.GetMainFrame().EvaluateScriptAsync("document.getElementById('ctl00_cp_wz_EnvioEE_fuArchivo').getBoundingClientRect()").ContinueWith(t =>
            {
                if (t.Result.Success)
                {
                    // Obtenemos las coordenadas
                    int top = (int)((Dictionary<string, object>)t.Result.Result)["top"];
                    int left = (int)((Dictionary<string, object>)t.Result.Result)["left"];

                    // Mandamos un click down a las coordenadas
                    browser.GetBrowser().GetHost().SendMouseClickEvent(left, top, MouseButtonType.Left, false, 1, CefEventFlags.None);
                    // Esperamos 100 ms para que reconozca el evento
                    Thread.Sleep(100);
                    // Mandamos un click up a las coordenadas
                    browser.GetBrowser().GetHost().SendMouseClickEvent(left, top, MouseButtonType.Left, true, 1, CefEventFlags.None);

                    // Enviamos el archivo
                    browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_wz_EnvioEE_StartNavigationTemplateContainerID_btnAceptar').click()");
                }
            });

            // Aceptamos el cargo en cuenta
            browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_wz_EnvioEE_StepNavigationTemplateContainerID_btnAceptarNav').click()");

            // Verificamos que se haya recibido el archivo
            browser.GetMainFrame().EvaluateScriptAsync("document.getElementById('ctl00_cp_wz_EnvioEE_FinishNavigationTemplateContainerID_btnVerEstadoArchivo').value").ContinueWith(t =>
            {
                if (t.Result.Success)
                {
                    // Cambiamos el estado en la BD
                    AEABLL.ChangeFileStatus(CurrentChargeAccount, ChargeAccountStatus.Uploaded);

                    // Cerramos sesion
                    browser.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('ctl00_TopButtonHome_lnkSalir').click()");
                }
            });
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
        /// Descarga un archivo desde una ruta ftp a una ruta especifica
        /// </summary>
        /// <param name="sourceFileName">Ruta ftp del archivo</param>
        /// <param name="destPath">Ruta especifica donde se descarga el archivo</param>
        /// <returns>La ruta del archivo descargado</returns>
        public string DownloadFtpFile(string sourceFileName, string destPath)
        {
            // Variables
            string fileName;
            string destFilePath;
            string FtpPath;
            string FtpUser;
            string FtpPass;

            try
            {
                // Obtenemos el nombre del archivo
                fileName = Path.GetFileName(sourceFileName);

                // Construimos la ruta destino
                destFilePath = destPath + @"\" + fileName;

                // Construimos la ruta ftp
                FtpPath = ConfigurationManager.AppSettings[ScraperBanescoPEResources.AppSettingsKeyFtpChargeAccount] +
                    @"/" + fileName;

                Logger.WriteSuccessLog(FtpPath, "ITCSS (Program)");

                // Obtenemos las credenciales
                FtpUser = ConfigurationManager.AppSettings[ScraperBanescoPEResources.AppSettingsKeyFtpChargeAccountUser];
                FtpPass = ConfigurationManager.AppSettings[ScraperBanescoPEResources.AppSettingsKeyFtpChargeAccountPass];

                #region FtpDownload
                // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-download-files-with-ftp
                // Get the object used to communicate with the server.  
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FtpPath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // Establecemos las credenciales del request
                request.Credentials = new NetworkCredential(FtpUser, FtpPass);

                // Obtenemos la respuesta del servidor ftp
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                // Creamos el archivo destino del cargo en cuenta y obtenemos el Stream de la respuesta
                // https://stackoverflow.com/questions/2934295/c-sharp-save-a-file-from-a-http-request
                using (Stream output = File.OpenWrite(destFilePath))
                using (Stream responseStream = response.GetResponseStream())
                {
                    // Copiamos el archivo de la respuesta ftp al archivo local del cargo en cuenta
                    responseStream.CopyTo(output);
                }

                Console.WriteLine("Download Complete, status {0}", response.StatusDescription);

                response.Close();
            }
            catch (Exception)
            {
                // Error

                throw;
            }

            // Success
            // Retornamos la ruta destino
            return destFilePath;

            #endregion
        }

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

        #region Obsolete

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

        #endregion
    }
}

