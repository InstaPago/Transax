using InstaTransfer.DataAccess;
using Fiddler;
using InstaTransfer.ITExceptions.Scraper;
using InstaTransfer.ITLogic;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ScraperContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using InstaTransfer.ITLogic.Security;
using System.Timers;
using InstaTransfer.ITResources.Scraper.ScraperBanesco;
using System.Security.Cryptography;
using BLL.Concrete;
using InstaTransfer.ITExceptions.Scraper.Banesco;

namespace InstaTransfer.ScraperPresenter
{
    /// <summary>
    /// Contiene todos los elementos comunes entre los diferentes Scrapers
    /// </summary>
    public class SPresenter
    {
        #region Variables

        //private static ISDateSelectionContract _dateSelectionView;

        //private SDateSelectionPresenter _dateSelectionPresenter;

        private ISContract _view;

        public Bank bankId;

        public bool _closeApp;

        public static PrivateFontCollection private_fonts = new PrivateFontCollection();

        public delegate WebBrowser GetWebBrowserHandler();

        public delegate WebBrowser ExitBrowserHandler();

        public delegate void NavigateParentBrowserHandler();

        public UUser _umbrellaUser;

        public string _accountNumber;

        public int _port;

        public System.Timers.Timer Timeout = new System.Timers.Timer();

        public UUserBLL UUBLL = new UUserBLL();

        //private const int CP_NOCLOSE_BUTTON = 0x200;
        #endregion

        #region Constructor
        public SPresenter(ISContract form)
        {
            _view = form;
            _view.TsmiDay.Click += new EventHandler(tsmiDay_Click);
            _view.WbUmbrellaExplorer.ProgressChanged += new WebBrowserProgressChangedEventHandler(wbUmbrellaExplorer_ProgressChanged);
            _view.WbUmbrellaExplorer.Navigated += new WebBrowserNavigatedEventHandler(wbUmbrellaExplorer_Navigated);
            _view.TsmiAccountBFI.Click += new EventHandler(tsmiAccountBFI_Click);
            _view.TsmiAccountTD.Click += new EventHandler(tsmiAccountTD_Click);
            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            // Definimos el Timer de cierre de la aplicacion
            Timeout.Elapsed += new ElapsedEventHandler(OnTimeout);
            double time;
            //Tomo el tiempo desde el app.config. Por defecto en 3m.
            try
            {
                //time = int.Parse(GeneralHelper.GetAppSettingValue(GeneralResources.AppConfigPath, GeneralResources.AppSettingsKeyScraperTimeout));
                time = int.Parse(GeneralHelper.GetAppSettingValue(GeneralResources.AppSettingsKeyScraperTimeout));
            }
            catch (Exception)
            {
                time = 180000;
            }
            Timeout.Interval = time;
            Timeout.Enabled = true;

            //Instalamos el certificado de fiddler
            FiddlerHelper.InstallCertificate();
            //Insertamos el SpringTextBox en ejecucion
            AddToolStripSpringTextBox();
            //Disable WebBroser user interaction
            ((Control)_view.WbUmbrellaExplorer).Enabled = true;
            //Desabilitamos la opcion por rango
            _view.TsmiRange.Enabled = false;
            _view.TsmiRange.Visible = false;
            //_view.TsmiRange.Click += new EventHandler(tsmiRange_Click);


        }

        public SPresenter()
        {

        }
        #endregion

        #region Events    

        private static void OnTimeout(object source, ElapsedEventArgs e)
        {
            Application.Exit();
        }

        private void tsmiDay_Click(object sender, EventArgs e)
        {
            ToggleDateOption(ScraperResources.ToggleDateOptionDay);
        }

        private void wbUmbrellaExplorer_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            UpdateStatusBar();
        }

        private void wbUmbrellaExplorer_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //Determinar el titulo de la ventana
            SetWindowTitle();
        }

        private void tsmiAccountBFI_Click(object sender, EventArgs e)
        {
            ToggleAccountOption(ScraperResources.ToggleAccountOptionBFI);
            _view.FillCredentials();
            FillEntryValues(_view.EntryValues[0], _view.EntryValues[1], _view.EntryValues[2]);
        }

        private void tsmiAccountTD_Click(object sender, EventArgs e)
        {
            ToggleAccountOption(ScraperResources.ToggleAccountOptionTD);
            _view.FillCredentials();
            FillEntryValues(_view.EntryValues[0], _view.EntryValues[1], _view.EntryValues[2]);

        }

        public virtual void FiddlerApplication_BeforeRequest(Session oSession) { }

        public virtual void FiddlerApplication_BeforeResponse(Session oSession) { }

        public void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            //Verifico que la respuesta http sea del tipo deseado
            if (oSession.oResponse.MIMEType == ScraperResources.MIMETypeExcel
                || oSession.oResponse.MIMEType == ScraperResources.MIMETypeText)
            {
                try
                {
                    // Variables
                    var currentUser = new UUser();
                    bool validationResult = false;

                    // Obtenemos la solicitud desde la bd
                    currentUser = UUBLL.GetUser(_umbrellaUser.Username, _umbrellaUser.IdUBank);                   

                    // Validamos el numero de cuenta de la solicitud
                    validationResult = ValidateBankAccount(currentUser, _accountNumber);

                    // Verificamos el resultado de la validacion
                    if (validationResult == true)
                    {
                        //Descargamos el archivo a la ubicacion correspondiente
                        DownloadFileToFolder(oSession);
                    }

                    // Validacion fallida
                    else
                    {
                        // Invalidamos la solicitud
                        currentUser.IdUserStatus = (int)UmbrellaUserStatus.InvalidBankAccount;
                        currentUser.StatusChangeDate = DateTime.Now;
                        // Guardamos los cambios en la base de datos
                        UUBLL.SaveChanges();
                        throw new InvalidBankAccountException(ScraperErrors.InvalidBankAccountExceptionCode, ScraperErrors.InvalidBankAccountExceptionMessage);
                    }
                }
                catch (InvalidBankAccountException e)
                {
                    ITLogic.Log.Logger.WriteErrorLog(e.MessageException, MethodBase.GetCurrentMethod().DeclaringType.Name);
                }
                catch (SException e)
                {
                    SException ex = new SException
                        (
                            ScraperErrors.SExceptionCode,
                            this.GetType().Name,
                            MethodBase.GetCurrentMethod().Name,
                            e.MessageException,
                            e
                        );
                    ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
                }
                catch (Exception e)
                {
                    SException ex = new SException
                        (
                            ScraperErrors.SExceptionCode,
                            this.GetType().Name,
                            MethodBase.GetCurrentMethod().Name,
                            ScraperErrors.SExceptionMessage,
                            e
                        );
                    ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
                }
                finally
                {
                    //Cerramos sesion
                    Logout(_umbrellaUser);
                }
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cerramos la conexion con Fiddler
            StopFiddler();
        }



        #endregion

        #region Methods

        /// <summary>
        /// Retorna si una solicitud es valida comparando los numeros de cuenta ingresados
        /// </summary>
        /// <param name="request">Solicitud de estado de cuenta</param>
        /// <param name="accountNumber">Numero de cuenta de la empresa</param>
        /// <returns></returns>
        public bool ValidateBankAccount(UUser user, string accountNumber)
        {
            // Comparamos el numero de cuenta de la base de datos con el de la plataforma bancaria
            if (UUBLL.GetBankAccount(user, "0134") == accountNumber)
            {
                // Valido
                return true;
            }
            // Invalido
            return false;
        }

        /// <summary>
        /// Obtiene el texto del <see cref="WebBrowser"/> actual
        /// </summary>
        /// <returns>El string del <see cref="WebBrowser"/></returns>
        public void NavigateParentBrowser()
        {
            if (_view.WbUmbrellaExplorer.InvokeRequired)
                _view.WbUmbrellaExplorer.Invoke(new NavigateParentBrowserHandler(NavigateParentBrowser));
            else
            {
                //Logout(_umbrellaUser);
            }
        }

        /// <summary>
        /// Cierra la sesion de un usuario especifico en la plataforma bancaria
        /// </summary>
        /// <param name="umbrellaUser">Usuario de la plataforma bancaria en uso</param>
        public void Logout(UUser umbrellaUser)
        {
            Command commandScraperLogout;

            try
            {
                commandScraperLogout = CommandFactory.GetCommandScraperLogout(umbrellaUser);
                commandScraperLogout.Parameter = new Dictionary<string, object>
                        {
                            { ScraperResources.DictionaryKeyBrowser, _view.WbUmbrellaExplorer }
                        };
                commandScraperLogout.Execute();

                _umbrellaUser = (UUser)commandScraperLogout.Receiver;
            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Obtiene el usuario asociado a un banco y razon social especifica.
        /// </summary>
        /// <param name="bankId">Id del banco asociado al usuario.</param>
        /// <param name="socialReasonId">Id de la razon social asociada al usuario.</param>
        /// <returns>Lista de <see cref="UUser"/> asocaido al banco y razon social especificada</returns>
        public List<UUser> GetUmbrellaUsers(string bankId, string socialReasonId)
        {
            Command commandGetUser;
            List<UUser> umbrellaUsers = new List<UUser>();

            try
            {
                commandGetUser = CommandFactory.GetCommandGetUser();
                commandGetUser.Parameter = new Dictionary<string, string>
                    {
                        { GeneralResources.DictionaryKeyBank, bankId },
                        { GeneralResources.DictionaryKeySocialReason, socialReasonId }
                    };

                commandGetUser.Execute();
                //Guardamos el resultado del comando como una lista de usuarios
                umbrellaUsers = (List<UUser>)commandGetUser.Receiver;
                //Retornamos la lista
                return umbrellaUsers;
            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
                return umbrellaUsers;
            }
        }

        /// <summary>
        /// Dispose del <see cref="WebBrowser"/> actual y creacion de uno nuevo
        /// </summary>
        /// <returns>El nuevo <see cref="WebBrowser"/></returns>
        public WebBrowser GetNewBrowser()
        {
            if (_view.WbUmbrellaExplorer.InvokeRequired)
                return _view.WbUmbrellaExplorer.Invoke(new GetWebBrowserHandler(GetNewBrowser)) as WebBrowser;
            else
            {

                _view.WbUmbrellaExplorer.Dispose();
                var browser = _view.WbUmbrellaExplorer;
                return browser;
            }
        }

        /// <summary>
        /// Construye la ruta de almacenamiento del EDC y el directorio si no existe.
        /// </summary>
        /// <param name="entryValues">Los parametros de entrada de la aplicacion.</param>
        /// <returns>La ruta de almacenamiento del EDC.</returns>
        /// <exception cref="IOException">Error en la ruta de la red</exception>
        public string BuildFilePathString(string[] entryValues)
        {
            string pathBuilder = "";
            try
            {
                //Directorio del proyecto.
                string startupPath = ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyBasePath];
                //Fecha y hora actual de descarga en formato compatible para archivos.
                string now = DateTime.Now.ToString(ScraperResources.DateFormat);
                //Construimos la ruta y el nombre del archivo
                string dBackslash = ScraperResources.DoubleBackslash.Replace(@"\\", @"\");
                //Construimos el nombre del archivo
                string fileName = now + entryValues[2];

                pathBuilder = string.Concat(startupPath
                    + dBackslash + entryValues[0]
                    + dBackslash + entryValues[1]
                    + dBackslash + fileName);
                //Construimos la ruta del directorio para su creacion
                string dirPath = string.Concat(startupPath
                    + dBackslash + entryValues[0]
                    + dBackslash + entryValues[1]
                    + dBackslash);
                //Creamos el directorio si no existe
                DirectoryInfo dir = Directory.CreateDirectory(dirPath);
            }
            catch (ArgumentException e)
            {
                SException ex = new SException
                    (
                        GeneralErrors.ArgumentExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
                ITLogic.Log.Logger.WriteErrorLog(ex, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (IOException e)
            {
                SException ex = new SException
                    (
                        GeneralErrors.IOExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        GeneralErrors.IOExceptionMessage,
                        e
                    );
                ITLogic.Log.Logger.WriteErrorLog(ex, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return pathBuilder;
        }

        /// <summary>
        /// Descarga el EDC con el parametro de sesion Http especificado
        /// a un archivo local
        /// </summary>
        /// <exception cref="SException">Error de Scraper</exception>
        public void DownloadFileToFolder(Session oSession)
        {
            try
            {
                //Limpiamos la respuesta Http
                oSession.utilDecodeResponse();
                //Guardamos la ruta de almacenamiento del EDC
                string filePath = BuildFilePathString(_view.EntryValues);
                //Almacenamos el cuerpo de la respuesta Http que contiene el EDC en la ruta especificada
                File.WriteAllBytes(filePath, oSession.responseBodyBytes);
                //Retornamos el resultado de la operacion
                ITLogic.Log.Logger.WriteSuccessLog("Archivo descargado: " + filePath, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (SException e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Llena los valores de entrada de la aplicacion segun banco y empresa
        /// </summary>
        /// <param name="bank">Banco origen de descarga del EDC</param>
        public void FillEntryValues(string socialReason, string bank, string fileFormat)
        {
            _view.EntryValues = new string[] { socialReason, bank, fileFormat };
        }

        /// <summary>
        /// Actualiza la barra de direcciones con el URL actual
        /// </summary>
        public void UpdateAddressBar()
        {
            _view.TsstbWebAddress.Text = _view.WbUmbrellaExplorer.Document.Url.ToString();
        }

        /// <summary>
        /// Agrega el elemento ToolStripSpringTextBox al ToolStrip de la ventana
        /// </summary>
        public void AddToolStripSpringTextBox()
        {
            _view.TsSBrowser.Items.Add(_view.TsstbWebAddress);
        }

        /// <summary>
        /// Realiza una acción de navegación a la dirección web especificada
        /// </summary>
        public void Navigate(string Url)
        {
            _view.WbUmbrellaExplorer.Navigate(Url);
        }

        /// <summary>
        /// Habilita la interaccion del usuario con la interfaz
        /// </summary>
        public void EnableUserInput()
        {
            _view.TstbUser.Enabled = true;
            _view.TstbPassword.Enabled = true;
            _view.TsbLogin.Enabled = true;
            _view.TsmiDates.Enabled = true;
            _view.TsmiAccount.Enabled = true;
        }

        /// <summary>
        /// Inhabilida la interaccion del usuario con la interfaz
        /// </summary>
        public virtual void DisableUserInput()
        {
            _view.TstbUser.Enabled = false;
            _view.TstbPassword.Enabled = false;
            _view.TsbLogin.Enabled = false;
            _view.TsmiDates.Enabled = false;
            _view.TsmiAccount.Enabled = false;
        }

        /// <summary>
        /// Alterna la seleccion de las opciones de fecha del dia o
        /// por rango.
        /// </summary>
        public void ToggleDateOption(string option)
        {
            if (_view.TsmiDay.Checked == false && option == ScraperResources.ToggleDateOptionDay)
            {
                _view.TsmiRange.Checked = false;
                _view.TsmiDay.Checked = true;
            }
            else if (_view.TsmiRange.Checked == false && option == ScraperResources.ToggleDateOptionRange)
            {
                _view.TsmiRange.Checked = true;
                _view.TsmiDay.Checked = false;
            }

        }

        /// <summary>
        /// Retorna el URL de la direccion web actual en formato string
        /// </summary>
        /// <returns>El URL de la direccion web actual</returns>
        public string GetCurrentUrlString()
        {
            string _currentURLString = _view.WbUmbrellaExplorer.Document.Url.ToString();
            return _currentURLString;
        }

        /// <summary>
        /// Actualiza la barra de estado con el estado actual de la pagina web
        /// </summary>
        public void UpdateStatusBar()
        {
            _view.TsslWebPageStatus.Text = _view.WbUmbrellaExplorer.StatusText;
        }

        /// <summary>
        /// Establece el titulo de la ventana segun el titulo de la pagina
        /// o el titulo por defecto
        /// </summary>
        public void SetWindowTitle()
        {
            if (_view.WbUmbrellaExplorer.DocumentTitle != string.Empty)
            {
                _view.SBrowser.Text = _view.WbUmbrellaExplorer.DocumentTitle;
            }
            else
            {
                _view.SBrowser.Text = ScraperResources.PageTitleDefault;
            }
        }

        /// <summary>
        /// Alterna la seleccion de las cuentas.
        /// </summary>
        public void ToggleAccountOption(string option)
        {
            if (_view.TsmiAccountTD.Checked == false && option == ScraperResources.ToggleAccountOptionTD)
            {
                _view.TsmiAccountTD.Checked = true;
                _view.TsmiAccountBFI.Checked = false;
            }
            else if (_view.TsmiAccountBFI.Checked == false && option == ScraperResources.ToggleAccountOptionBFI)
            {
                _view.TsmiAccountTD.Checked = false;
                _view.TsmiAccountBFI.Checked = true;
            }
        }

        public static void LoadCustomFont()
        {
            // Use this if you can not find your resource System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string resource = ScraperResources.CustomFontPath;
            // receive resource stream
            Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

            //create an unsafe memory block for the data 
            IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);
            //create a buffer to read in to
            Byte[] fontData = new Byte[fontStream.Length];
            //fetch the font program from the resource
            fontStream.Read(fontData, 0, (int)fontStream.Length);
            //copy the bytes to the unsafe memory block
            Marshal.Copy(fontData, 0, data, (int)fontStream.Length);

            // We HAVE to do this to register the font to the system (Weird .NET bug !)
            uint cFonts = 0;
            SNativeMethods.AddFontMemResourceEx(data, (uint)fontData.Length, IntPtr.Zero, ref cFonts);
            //pass the font to the font collection
            private_fonts.AddMemoryFont(data, (int)fontStream.Length);
            //close the resource stream
            fontStream.Close();
            //free the unsafe memory
            Marshal.FreeCoTaskMem(data);
        }

        /// <summary>
        /// Levanta la aplicacion de fiddler en el puerto por defecto.
        /// </summary>
        public void StartFiddler()
        {
            try
            {
                //Creamos el handler del evento que se dispara para los diferentes eventos de la sesion
                FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
                FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
                FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;
                //Iniciamos fiddler
                //FiddlerHelper.StartFiddler(_port);
                FiddlerHelper.StartFiddler(8888);

                ITLogic.Log.Logger.WriteSuccessLog("Fiddler inició correctamente", MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception)
            {
                ITLogic.Log.Logger.WriteErrorLog(GeneralErrors.FiddlerExceptionMessage, MethodBase.GetCurrentMethod().DeclaringType.Name);
                Logout(_umbrellaUser);
            }

        }

        /// <summary>
        /// Cierra la sesion de fiddler
        /// </summary>
        public void StopFiddler()
        {
            //Removemos el evento de captura de la sesion
            FiddlerApplication.AfterSessionComplete -= FiddlerApplication_AfterSessionComplete;
            FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
            //Detenemos fiddler
            FiddlerHelper.StopFiddler();
        }


        #endregion

        #region Obsolete

        /// <summary>
        /// Inicia sesion en la plataforma bancaria para un usuario especifico
        /// </summary>
        /// <param name="umbrellaUser">Usuario de la plataforma bancaria</param>
        [Obsolete("Se utiliza el login dentro de BanescoPresenter")]
        public void Login(UUser umbrellaUser)
        {
            Command commandScraperLogin;
            Command commandChangeUserStatus;

            try
            {
                commandScraperLogin = CommandFactory.GetCommandScraperLogin(umbrellaUser);
                commandScraperLogin.Parameter = new Dictionary<string, object>
                        {
                            { ScraperResources.DictionaryKeyBrowser, _view.WbUmbrellaExplorer }
                        };
                commandScraperLogin.Execute();

                commandChangeUserStatus = CommandFactory.GetCommandChangeUserStatus(umbrellaUser);
                commandChangeUserStatus.Parameter = new Dictionary<string, object>
                {
                    {GeneralResources.DictionaryKeyUserStatus , UmbrellaUserStatus.InUse }
                };
                commandChangeUserStatus.Execute();
                _umbrellaUser = (UUser)commandChangeUserStatus.Receiver;
            }
            catch (CryptographicException)
            {
                commandChangeUserStatus = CommandFactory.GetCommandChangeUserStatus(umbrellaUser);
                commandChangeUserStatus.Parameter = new Dictionary<string, object>
                {
                    {GeneralResources.DictionaryKeyUserStatus , UmbrellaUserStatus.DecryptError }
                };
                commandChangeUserStatus.Execute();

                Application.ExitThread();
            }
            catch (UserLoginException e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
                Application.ExitThread();

            }
            catch (Exception e)
            {
                ITLogic.Log.Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Verifica si existe el certificado y public key en el app.config.
        /// Si existe la guarda en las preferencias de fiddler
        /// </summary>
        /// <returns>True - Existe el certificado. False - No existe el certificado.</returns>
        [Obsolete("Los certificados se almacenan en CertMaker, por lo que nunca se guardan en FiddleApplication.Prefs")]
        private bool CheckCertificate()
        {
            if (!string.IsNullOrEmpty(ITSecurity.RetrieveSecureCredentials(ScraperResources.SettingsFiddlerCert)))
            {
                FiddlerApplication.Prefs.SetStringPref(ScraperResources.PrefFiddlerPK, ITSecurity.RetrieveSecureCredentials(ScraperResources.SettingsFiddlerPK));
                FiddlerApplication.Prefs.SetStringPref(ScraperResources.PrefFiddlerCert, ITSecurity.RetrieveSecureCredentials(ScraperResources.SettingsFiddlerCert));
                return true;
            }
            return false;
        }

        [Obsolete("Se elimina el funcionamiento por rangos del Scraper")]
        /// <summary>
        /// Levanta la ventana de seleccion de fechas y captura los datos de las fechas
        /// </summary>
        private void InputRange()
        {

            ////Alterna los estados de las opciones de las fechas
            //ToggleDateOption(ScraperResources.ToggleDateOptionRange);

            //DialogResult dialogResult = _dateSelectionView.SDateSelector.ShowDialog();
            //if (dialogResult == DialogResult.OK)
            //{
            //    _view.StartDate = _dateSelectionView.StartDate;
            //    _view.EndDate = _dateSelectionView.EndDate;
            //}
            //else if (dialogResult == DialogResult.Cancel)
            //{
            //    //Do nothing
            //}
            //_dateSelectionView.SDateSelector.Dispose();
        }

        #endregion
    }
}
