using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using InstaTransfer.ITResources.Scraper.ScraperProvincial;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace InstaTransfer.ITLogic.Commands.Scraper.Banesco
{
    public class CommandProvincialScraperLogout : Command
    {
        public CommandProvincialScraperLogout(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            Command commandChangeUserStatus;

            UUser currentUser = (UUser)Receiver;
            Dictionary<string, object> parameter = (Dictionary<string, object>)Parameter;
            WebBrowser browser = (WebBrowser)parameter[ScraperResources.DictionaryKeyBrowser];

            try
            {
                browser.Navigate(ScraperProvincialResources.URLLogin);

                commandChangeUserStatus = CommandFactory.GetCommandChangeUserStatus(currentUser);
                commandChangeUserStatus.Parameter = new Dictionary<string, object>
                {
                    {GeneralResources.DictionaryKeyUserStatus , UmbrellaUserStatus.Active }
                };
                commandChangeUserStatus.Execute();
                currentUser = (UUser)commandChangeUserStatus.Receiver;

                Receiver = currentUser;
            }

            catch (NullReferenceException e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
