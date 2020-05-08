using System.Windows.Forms;
using InstaTransfer.ScraperPresenter.Componentes;
using InstaTransfer.ScraperContracts.Componentes;

namespace InstaTransfer.Scraper.Componentes
{
    /// <summary>
    /// Ventana de seleccion del rango de fechas
    /// </summary>
    public partial class SBankSelection : Form, ISBankSelectionContract
    {
        #region Presenter

        private SBankSelectionPresenter _presenter;

        #endregion

        #region Model

        public Button BBanesconline
        {
            get { return bBanesconline; }

            set { bBanesconline = value; }
        }

        public Button BProvinet
        {
            get { return bProvinet; }

            set { bProvinet = value; }
        }

        #endregion


        //private void bBanesconline_Click(object sender, EventArgs e)
        //{
        //    SBrowserBanesco scraperBanesco = new SBrowserBanesco();
        //    this.Hide();
        //    scraperBanesco.Show();
        //}

        //private void bProvinet_Click(object sender, EventArgs e)
        //{
        //    SBrowserProvincial scraperProvincial = new SBrowserProvincial();
        //    this.Hide();
        //    scraperProvincial.Show();           
        //}

        #region Constructor

        public SBankSelection()
        {
            InitializeComponent();
            _presenter = new SBankSelectionPresenter(this);


            //if (args.Count() > 0)
            //{
            //    var scraper = ScraperFactory.GetScraper(GeneralHelper.GetBankEnum(args[0]));
            //    scraper.Show();
            //}
            //Logica de inicio de scrapers
        }

        #endregion

    }
}
