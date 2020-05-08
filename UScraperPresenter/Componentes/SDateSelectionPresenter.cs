using InstaTransfer.ScraperContracts.Componentes;

namespace InstaTransfer.ScraperPresenter.Componentes
{
    public class SDateSelectionPresenter
    {
        private ISDateSelectionContract _view;
        public SDateSelectionPresenter(ISDateSelectionContract viewSDateSelection)
        {
            _view = viewSDateSelection;
        }
    }
}