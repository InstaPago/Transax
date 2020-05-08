using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Scraper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using InstaTransfer.ITResources.Scraper.ScraperBanesco2;

namespace InstaTransfer.ITLogic.Commands.Scraper.Banesco2
{
    public class CommandBanesco2ScraperLogout : Command
    {
        public CommandBanesco2ScraperLogout(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            Command commandChangeUserStatus;

            UUser currentUser = (UUser)Receiver;
            Dictionary<string, object> parameter = (Dictionary<string, object>)Parameter;
            WebBrowser browser = (WebBrowser)parameter[ScraperResources.DictionaryKeyBrowser];

            try
            {
                commandChangeUserStatus = CommandFactory.GetCommandChangeUserStatus(currentUser);
                commandChangeUserStatus.Parameter = new Dictionary<string, object>
                {
                    {GeneralResources.DictionaryKeyUserStatus , UmbrellaUserStatus.Active }
                };
                commandChangeUserStatus.Execute();
                currentUser = (UUser)commandChangeUserStatus.Receiver;

                Receiver = currentUser;

                browser.Navigate(ScraperBanesco2Resources.URLSalir);
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
