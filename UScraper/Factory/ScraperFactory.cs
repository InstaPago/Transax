using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.Scraper.Banesco;
using InstaTransfer.Scraper.Provincial;
using System;
using System.Windows.Forms;
using InstaTransfer.Scraper.Banesco2;

namespace InstaTransfer.ScraperView.Factory
{
    /// <summary>
    /// Fabrica que instancia los scrapers de cada banco
    /// </summary>
    public class ScraperFactory
    {
        public static Form GetScraper(object receiver)
        {
            UUser user = (UUser)receiver;
            Bank bank = GeneralHelper.GetBankEnum(user.IdUBank);
            var ramdom = new Random();
            var port = ramdom.Next(8888, 20000);

            switch (bank)
            {
                case Bank.Banesco:
                    {
                        /// BanescOnline antiguo.
                        /// return new SBrowserBanesco(user, port);
                        return new SBrowserBanesco2(user, port);
                    }
                case Bank.Provincial:
                    {
                        return new SBrowserProvincial(user);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
