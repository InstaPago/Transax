using System.Windows.Forms;
using InstaTransfer.ScraperContracts.Componentes;
using InstaTransfer.ITResources.Scraper;
using System;
using InstaTransfer.ScraperPresenter.Componentes;

namespace InstaTransfer.Scraper
{
    /// <summary>
    /// Ventana de seleccion del rango de fechas
    /// </summary>
    public partial class SDateSelection : Form, ISDateSelectionContract
    {
        #region Presenter
        private SDateSelectionPresenter _presenter;
        #endregion

        public string startDate = string.Empty, endDate = string.Empty;

        public SDateSelection()
        {
            InitializeComponent();
            _presenter = new SDateSelectionPresenter(this);
        }

        private SDateSelection _sDateSelector;

        #region Model
        public Form SDateSelector
        {
            get
            {
                if (_sDateSelector == null)
                {
                    _sDateSelector = new SDateSelection();
                }

                return _sDateSelector;
            }
        }

        public Button BDateRangeAccept
        {
            get { return bDateRangeAccept; }

            set { bDateRangeAccept = value; }
        }

        public Button BDateRangeCancel
        {
            get { return bDateRangeCancel; }

            set { bDateRangeCancel = value; }
        }

        public DateTimePicker DtpDateEnd
        {
            get { return dtpDateEnd; }

            set { dtpDateEnd = value; }
        }

        public DateTimePicker DtpDateStart
        {
            get { return dtpDateStart; }

            set { dtpDateStart = value; }
        }

        public Label LDateEnd
        {
            get { return lDateEnd; }

            set { lDateEnd = value; }
        }

        public Label LDateStart
        {
            get { return lDateStart; }

            set { lDateStart = value; }
        }

        public string StartDate
        {
            get { return startDate; }

            set { startDate = value; }
        }

        public string EndDate
        {
            get { return endDate; }

            set { endDate = value; }
        }
        #endregion

        private void bDateRangeAccept_Click(object sender, EventArgs e)
        {
            StartDate = DtpDateStart.Value.Date.ToString(ScraperResources.ShortDateFormat).Replace(ScraperResources.CharDash, ScraperResources.CharSlash);
            EndDate = DtpDateEnd.Value.Date.ToString(ScraperResources.ShortDateFormat).Replace(ScraperResources.CharDash, ScraperResources.CharSlash);
        }
    }
}
