using Fiddler;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITResources.Scraper.ScraperProvincial;
using InstaTransfer.ScraperContracts.Banco.Provincial;
using System;
using System.Drawing;
using System.Windows.Forms;
using InstaTransfer.ITLogic.Security;

namespace InstaTransfer.ScraperPresenter.Banco.Provincial
{
    /// <summary>
    /// Metodos exclusivos para cada Scraper
    /// </summary>
    public class SProvincialPresenter : SPresenter
    {
        #region Variables

        private ISProvincialContract _view;

        enum AccountField { PUserBFI, PPWBFI, PRIFBFI, PUserTD, PPWTD, PRIFTD }

        #endregion

        #region Constructor
        public SProvincialPresenter(ISProvincialContract viewUScraperProvincial) :
            base(viewUScraperProvincial)
        {
            _view = viewUScraperProvincial;
            _view.WbUmbrellaExplorer.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wbUmbrellaExplorer_DocumentCompleted);
            _view.TsbHome.Click += new EventHandler(tsbHome_Click);
            _view.TsmiLogout.Click += new EventHandler(tsmiLogout_Click);
            _view.TsbLogin.Click += new EventHandler(tsbLogin_Click);
            _view.SBrowser.Load += new EventHandler(Browser_Load);
        }
        #endregion

        #region Events
        private void Browser_Load(object sender, EventArgs e)
        {
            //Initialize toolstrip custom properties
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
            _view.WbUmbrellaExplorer.Url = new Uri(ScraperProvincialResources.URLLogin);
            _view.WbUmbrellaExplorer.ScriptErrorsSuppressed = true;
            FillEntryValues(_umbrellaUser.IdUSocialReason, ScraperResources.BankOptionProvincial, ScraperResources.FileExtensionCsv);
            bankId = Bank.Provincial;

        }

        private void wbUmbrellaExplorer_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlDocument _doc = ((WebBrowser)sender).Document;


            //Actualizar la dirección web de la barra de direcciones.
            UpdateAddressBar();
            //Si se encuentra en la pagina de login, habilita el user input
            if (GetCurrentUrlString() == ScraperProvincialResources.URLLogin)
            {
                Login(_umbrellaUser);
            }
            //Navegar automaticamente a la pagina de cuentas y movimientos.
            if (InvokeHtmlElementFromFrame(_doc, ScraperProvincialResources.HtmlIDCuentas))
            {
                Console.WriteLine("Elemento seleccionado: {0}", ScraperProvincialResources.HtmlIDCuentas);
                DisableUserInput();
                InvokeHtmlElementFromFrame(ScraperProvincialResources.HtmlNameFramePrincipal, _doc);
            }
            else if (InvokeHtmlElementFromFrame(_doc, ScraperProvincialResources.HtmlNameDescargar))
            {
                HtmlWindow frame = GetFrame(ScraperProvincialResources.HtmlNameFramePrincipal, _doc);
                Console.WriteLine("Elemento seleccionado: {0}", ScraperProvincialResources.HtmlNameDescargar);
                if (_view.TsmiRange.Checked)
                {
                    SelectRange(_view.StartDate, _view.EndDate, frame.Document);
                    DownloadFileFromFrame(frame);
                }
                else
                {
                    SelectDay(frame.Document);
                    DownloadFileFromFrame(frame);
                }

            }
            else if (InvokeHtmlElementFromFrame(_doc, ScraperProvincialResources.HtmlNameCuentas))
            {
                Console.WriteLine("Elemento seleccionado: {0}", ScraperProvincialResources.EmptyString);
            }
            else if (GetCurrentUrlString() == ScraperProvincialResources.URLSalir)
            {
                Application.ExitThread();
            }

            if (_closeApp == true)
            {
                Application.ExitThread();
            }
        }

        private void tsbHome_Click(object sender, EventArgs e)
        {
            GoHome();
        }

        private void tsbLogin_Click(object sender, EventArgs e)
        {
            ////Segun la cuenta seleccionada, guardamos en el App.config las credenciales ingresadas
            ////por el usuario y luego iniciamos sesion en la pagina con las credenciales almacenadas.
            //if (_view.TsmiAccountBFI.Checked)
            //{
            //    ITSecurity.StoreSecureCredentials(_view.TstbUser.Text, AccountField.PUserBFI.ToString());
            //    ITSecurity.StoreSecureCredentials(_view.TstbPassword.Text, AccountField.PPWBFI.ToString());
            //    ITSecurity.StoreSecureCredentials(_view.TstbRIF.Text, AccountField.PRIFBFI.ToString());
            //    Login(AccountField.PUserBFI.ToString(), AccountField.PPWBFI.ToString(), AccountField.PRIFBFI.ToString());
            //}
            //else if (_view.TsmiAccountTD.Checked)
            //{
            //    ITSecurity.StoreSecureCredentials(_view.TstbUser.Text, AccountField.PUserTD.ToString());
            //    ITSecurity.StoreSecureCredentials(_view.TstbPassword.Text, AccountField.PPWTD.ToString());
            //    ITSecurity.StoreSecureCredentials(_view.TstbRIF.Text, AccountField.PRIFTD.ToString());
            //    Login(AccountField.PUserTD.ToString(), AccountField.PPWTD.ToString(), AccountField.PRIFTD.ToString());
            //}
        }

        private void tsmiLogout_Click(object sender, EventArgs e)
        {
            Logout(_umbrellaUser);
            Application.ExitThread();
        }

        public override void FiddlerApplication_BeforeResponse(Session oSession)
        {
            if (oSession.oResponse.MIMEType == ScraperResources.MIMETypeExcel)
            {
                GetNewBrowser();
            }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Navega el frame especificado del documento e invoca un htmlelement segun sea el caso
        /// </summary>
        /// <param name="frameName"><see cref="HtmlWindow"/> a recorrer</param>
        /// <param name="doc"><see cref="HtmlDocument"/> que contiene los <see cref="HtmlWindow"/> a recorrer</param>
        private void InvokeHtmlElementFromFrame(string frameName, HtmlDocument doc)
        {
            //Click al link de cuentas de la pagina
            GetFrame(frameName, doc)
                .Document
                .Links[5]
                .InvokeMember(ScraperResources.HtmlMemberClick);
        }

        /// <summary>
        /// Navega los frames del documento y selecciona un htmlelement por el id indicado
        /// </summary>
        /// <param name="doc"><see cref="HtmlDocument"/> que contiene los <see cref="HtmlWindow"/> a recorrer</param>
        /// <param name="elementId">Id del <see cref="HtmlElement"/> a invocar</param>
        /// <returns></returns>
        private bool InvokeHtmlElementFromFrame(HtmlDocument doc, string elementId)
        {
            HtmlElement element = null;
            if (!(doc == null))
            {
                HtmlWindow currentWindow = doc.Window;
                if (currentWindow.Frames.Count > 0)
                {
                    foreach (HtmlWindow frame in currentWindow.Frames)
                    {
                        element = frame.Document.GetElementById(elementId);
                        if (element != null)
                        {
                            element.InvokeMember(ScraperProvincialResources.HtmlMemberClick);
                            return true;
                        }
                        else if (frame.Document.Links.Count != 0 && frame.Url.AbsolutePath == ScraperProvincialResources.URLAbsolutePath &&
                            elementId == ScraperProvincialResources.HtmlNameCuentas)
                        {
                            element = frame.Document.Links[5];
                            element.InvokeMember(ScraperProvincialResources.HtmlMemberClick);
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Cierra la sesion del usuario del banco y retorna a la pagina de login
        /// </summary>
        public void GoHome()
        {
            //Contener todo esto en un comando
            if (_view.WbUmbrellaExplorer.Url.ToString() != ScraperProvincialResources.URLLogin)
            {
                Logout(_umbrellaUser);
                //EnableUserInput();
            }
        }

        /// <summary>
        /// Habilita la interaccion del usuario con la interfaz del UScraperProvincial
        /// </summary>
        public new void EnableUserInput()
        {
            _view.TstbUser.Enabled = true;
            _view.TstbPassword.Enabled = true;
            _view.TsbLogin.Enabled = true;
            _view.TsmiDates.Enabled = true;
            _view.TsmiAccount.Enabled = true;
            _view.TstbRIF.Enabled = true;
        }

        /// <summary>
        /// Inhabilida la interaccion del usuario con la interfaz
        /// </summary>
        public override void DisableUserInput()
        {
            base.DisableUserInput();
            _view.TstbRIF.Enabled = false;
        }

        /// <summary>
        /// Descarga el EDC desde el Frame especificado
        /// </summary>
        public void DownloadFileFromFrame(HtmlWindow frame)
        {
            //Click al boton de descargar de la pagina
            var htmlInputList = frame.Document.GetElementsByTagName(ScraperResources.HtmlTagInput);
            foreach (HtmlElement input in htmlInputList)
            {
                if (input.Name == ScraperProvincialResources.HtmlNameDescargar)
                {
                    StartFiddler();
                    input.InvokeMember(ScraperResources.HtmlMemberClick);
                }
            }
        }

        /// <summary>
        /// Cierra sesión si ya existe una sesion abierta. Cierra la aplicacion por defecto.
        /// </summary>
        //private void Logout(bool closeApp)
        //{
        //    //Cierra la aplicacion si no se ha iniciado sesion.
        //    if ((_view.WbUmbrellaExplorer.Url.ToString().Contains(ScraperProvincialResources.URLLogin2) ||
        //        _view.WbUmbrellaExplorer.Url.ToString().Contains(ScraperProvincialResources.URLLogin)) && closeApp)
        //    {
        //        Application.Exit();
        //    }
        //    //Cierra sesion si existe una abierta, de lo contrario cierra la aplicacion.
        //    else
        //    {
        //        try
        //        {
        //            var _doc = _view.WbUmbrellaExplorer.Document;
        //            var _frame = GetFrame(ScraperProvincialResources.HtmlNameFrameCabecera, _doc);
        //            _frame.Document.Links[0]
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
        //                Navigate(ScraperProvincialResources.URLSalir);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Seleccion de visualizacion por rango del EDC. Ingresa parametros 
        /// del rango de fechas en la pagina web.
        /// </summary>
        /// <param name="startDate">Fecha inicio del EDC</param>
        /// <param name="endDate">Fecha fin del EDC</param>
        /// <param name="doc">Documento html de la pagina a consultar</param>
        private void SelectRange(string startDate, string endDate, HtmlDocument doc)
        {
            //Seleccionar la opcion de ver EDC por rango
            HtmlElementCollection htmlInputList = doc.GetElementsByTagName(ScraperResources.HtmlTagInput);
            htmlInputList[21].InvokeMember(ScraperResources.HtmlMemberClick);
            //Ingresa la fecha de inicio seleccionada en el textbox de la pagina
            var fechainicio = doc.GetElementById(ScraperProvincialResources.HtmlIDFechaInicio);
            fechainicio.SetAttribute(ScraperResources.HtmlAttributeValue, startDate);
            //Ingresa la fecha fin seleccionada en el textbox de la pagina
            var fechafin = doc.GetElementById(ScraperProvincialResources.HtmlIDFechaFin);
            fechafin.SetAttribute(ScraperResources.HtmlAttributeValue, endDate);
            //Selecciona el boton de consultar EDC en la pagina web
            htmlInputList[26].InvokeMember(ScraperResources.HtmlMemberClick);
        }

        /// <summary>
        /// Selecciona la opcion de EDC del dia en la pagina web
        /// </summary>
        /// <param name="doc">Documento Html de la pagina</param>
        private void SelectDay(HtmlDocument doc)
        {
            HtmlElementCollection htmlInputList = doc.GetElementsByTagName(ScraperResources.HtmlTagInput);
            htmlInputList[19].InvokeMember(ScraperResources.HtmlMemberClick);
            htmlInputList[26].InvokeMember(ScraperResources.HtmlMemberClick);
        }

        /// <summary>
        /// Llena los TextBox de las credenciales si fueron guardadas previamente en el App.config
        /// </summary>
        public void FillCredentials()
        {
            if (_view.TsmiAccountBFI.Checked)
            {
                _view.TstbUser.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PUserBFI.ToString());
                _view.TstbPassword.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PPWBFI.ToString());
                _view.TstbRIF.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PRIFBFI.ToString());
            }
            else if (_view.TsmiAccountTD.Checked)
            {
                _view.TstbUser.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PUserTD.ToString());
                _view.TstbPassword.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PPWTD.ToString());
                _view.TstbRIF.Text = ITSecurity.RetrieveSecureCredentials(AccountField.PRIFTD.ToString());
            }
        }

        /// <summary>
        /// Retorna el <see cref="HtmlWindow"/> con el nombre especificado
        /// </summary>
        /// <param name="name"> Nombre del Frame a buscar</param>
        /// <param name="doc"><see cref="HtmlDocument"/> donde se encuentra el Frame</param>
        /// <returns>El elemento <see cref="HtmlWindow"/></returns>
        private HtmlWindow GetFrame(string name, HtmlDocument doc)
        {
            HtmlWindow _frame = doc.Window.Frames[name];
            return _frame;
        }

        #endregion
    }
}