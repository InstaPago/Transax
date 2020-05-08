using InstaTransfer.ScraperContracts.Componentes;

namespace InstaTransfer.ScraperPresenter.Componentes
{
    public class SBankSelectionPresenter
    {
        private ISBankSelectionContract _view;
        public SBankSelectionPresenter(ISBankSelectionContract viewBankSelection)
        {
            _view = viewBankSelection;
        }
    }
}