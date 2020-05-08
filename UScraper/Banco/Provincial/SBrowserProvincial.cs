using System;
using System.Windows.Forms;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ScraperContracts.Banco.Provincial;
using InstaTransfer.ScraperContracts;
using InstaTransfer.ScraperPresenter.Banco.Provincial;
using InstaTransfer.ITResources.Scraper.ScraperProvincial;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.Enums;

namespace InstaTransfer.Scraper.Provincial
{
    public partial class SBrowserProvincial : Form, ISProvincialContract
    {
        #region Presenter
        private SProvincialPresenter _presenter;
        #endregion

        #region Model
        private string startDate;
        public string StartDate
        {
            get { return startDate; }

            set { startDate = value; }
        }

        private string endDate;
        public string EndDate
        {
            get { return endDate; }

            set { endDate = value; }
        }

        private string[] entryValues;
        public string[] EntryValues
        {
            get { return entryValues; }

            set { entryValues = value; }
        }

        private ToolStripSpringTextBox tsstbWebAddress = new ToolStripSpringTextBox();
        ToolStripSpringTextBox ISContract.TsstbWebAddress
        {
            get { return tsstbWebAddress; }

            set { tsstbWebAddress = value; }
        }

        public Form SBrowser
        {
            get { return this; }
        }

        public WebBrowser WbUmbrellaExplorer
        {
            get {
                if (wbUmbrellaExplorer.IsDisposed)
                {
                    wbUmbrellaExplorer = new WebBrowser();
                    this.wbUmbrellaExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
                    this.wbUmbrellaExplorer.Location = new System.Drawing.Point(1, 31);
                    this.wbUmbrellaExplorer.MinimumSize = new System.Drawing.Size(20, 20);
                    this.wbUmbrellaExplorer.Name = "wbUmbrellaExplorer";
                    this.wbUmbrellaExplorer.Size = new System.Drawing.Size(821, 672);
                    this.wbUmbrellaExplorer.TabIndex = 0;
                    this.wbUmbrellaExplorer.Url = new System.Uri(ScraperProvincialResources.URLLogin, System.UriKind.Absolute);
                    this.wbUmbrellaExplorer.ScriptErrorsSuppressed = true;
                    _presenter._closeApp = true;
                    SBrowser.Controls.Add(this.wbUmbrellaExplorer);
                }
                return wbUmbrellaExplorer; }

            set { wbUmbrellaExplorer = value; }
        }

        public ToolStrip TsSBrowser
        {
            get { return tsUBrowser; }

            set { tsUBrowser = value; }
        }

        public ToolStripTextBox TstbUser
        {
            get { return tstbUser; }

            set { tstbUser = value; }
        }

        public ToolStripTextBox TstbPassword
        {
            get { return tstbPassword; }

            set { tstbPassword = value; }
        }

        public ToolStripButton TsbLogin
        {
            get { return tsbLogin; }

            set { tsbLogin = value; }
        }

        public ToolStripMenuItem TsmiLogout
        {
            get { return tsmiLogout; }

            set { tsmiLogout = value; }
        }

        public ToolStripMenuItem TsmiDates
        {
            get { return tsmiDates; }

            set { tsmiDates = value; }
        }

        public ToolStripMenuItem TsmiAccount
        {
            get { return tsmiAccount; }

            set { tsmiAccount = value; }
        }

        public ToolStripTextBox TstbRIF
        {
            get { return tstbRIF; }

            set { tstbRIF = value; }
        }

        public ToolStripMenuItem TsmiDay
        {
            get { return tsmiDay; }

            set { tsmiDay = value; }
        }
   
        public ToolStripMenuItem TsmiRange
        {
            get { return tsmiRange; }

            set { tsmiRange = value; }
        }

        public ToolStripMenuItem TsmiAccountTD
        {
            get { return tsmiAccountTD; }

            set { tsmiAccountTD = value; }
        }

        public ToolStripMenuItem TsmiAccountBFI
        {
            get { return tsmiAccountBFI; }

            set { tsmiAccountBFI = value; }
        }

        public ToolStripStatusLabel TsslWebPageStatus
        {
            get { return tsslWebPageStatus; }

            set { tsslWebPageStatus = value; }
        }

        public ToolStripButton TsbHome
        {
            get { return tsbHome; }

            set { tsbHome = value; }
        }

        public ToolStripDropDownButton TsddbMenu
        {
            get { return tsddbMenu; }

            set { tsddbMenu = value; }
        }

        public ToolStripLabel TslPassword
        {
            get { return tslPassword; }

            set { tslPassword = value; }
        }

        public ToolStripLabel TslUser
        {
            get { return tslUser; }

            set { tslUser = value; }
        }
        #endregion

        #region Constructor
        public SBrowserProvincial(UUser user)
        {
            InitializeComponent();
            _presenter = new SProvincialPresenter(this);
            _presenter._umbrellaUser = user;
            //Insert Web Address text box at runtime
            _presenter.AddToolStripSpringTextBox();

            Application.ThreadExit += new EventHandler(Application_ThreadExit);
        }
        #endregion

        #region Testing
        private const int CP_NOCLOSE_BUTTON = 0x200;

        private void tsmiRange_Click(object sender, EventArgs e)
        {
            InputRange();
        }

        private void InputRange()
        {
            //Alterna los estados de las opciones de las fechas
            _presenter.ToggleDateOption(ScraperResources.ToggleDateOptionRange);

            SDateSelection SDateSelectionPopUp = new SDateSelection();
            //DialogResult dialogResult = SDateSelectionPopUp.ShowDialog();
            DialogResult dialogResult = SDateSelectionPopUp.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                StartDate = SDateSelectionPopUp.StartDate;
                EndDate = SDateSelectionPopUp.EndDate;
            }
            else if (dialogResult == DialogResult.Cancel)
            {
                //Do nothing
            }
            SDateSelectionPopUp.Dispose();
        }

        public void FillCredentials()
        {
            _presenter.FillCredentials();
        }

        public string BuildFilePathString(string[] entryValues)
        {
            return _presenter.BuildFilePathString(EntryValues);
        }

        /// <summary>
        /// Inhabilita el boton de cierre de la ventana
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }


        #endregion

        //Todo(SBrowserProvincial): Mover al presentador correspondiente
        private void SBrowserProvincial_ControlAdded(object sender, ControlEventArgs e)
        {
             Application.ExitThread();
        }
        //Todo(SBrowserProvincial): Por favor mejorar esto.
        private void Application_ThreadExit(Object sender, EventArgs e)
        {
            if (_presenter._umbrellaUser.IdUserStatus == 5 && _presenter._umbrellaUser.IdUBank == GeneralHelper.GetBankIdString(Bank.Provincial))
                _presenter.Logout(_presenter._umbrellaUser);

        }
    }
}
