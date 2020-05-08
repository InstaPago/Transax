using System.Windows.Forms;
using InstaTransfer.ScraperContracts.Banco.Banesco;
using InstaTransfer.ScraperPresenter.Banco.Banesco;
using InstaTransfer.ScraperContracts;
using InstaTransfer.DataAccess;
using InstaTransfer.ScraperPresenter.Banco.Banesco2;
using InstaTransfer.ScraperContracts.Banco.Banesco2;

namespace InstaTransfer.Scraper.Banesco2
{
    public partial class SBrowserBanesco2 : Form, ISBanesco2Contract
    {
        #region Presenter
        private SBanesco2Presenter _presenter;
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

        ToolStripSpringTextBox tsstbWebAddress = new ToolStripSpringTextBox();
        public ToolStripSpringTextBox TsstbWebAddress
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
            get { return wbUmbrellaExplorer; }

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

        public void FillCredentials()
        {
            _presenter.FillCredentials();
        }
        public string BuildFilePathString(string[] entryValues)
        {
            return _presenter.BuildFilePathString(EntryValues);
        }
        #endregion

        #region Constructor
        public SBrowserBanesco2(UUser user, int port)
        {
            InitializeComponent();
            _presenter = new SBanesco2Presenter(this);
            _presenter._umbrellaUser = user;
            _presenter._port = port;
            
        }
        #endregion

        #region Testing
        private const int CP_NOCLOSE_BUTTON = 0x200;


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
    }
}
