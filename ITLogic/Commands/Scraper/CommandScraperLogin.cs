using InstaTransfer.ITLogic.Log;
using System;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Scraper
{
    public class CommandScraperLogin : Command
    {

        public CommandScraperLogin(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            try
            {

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
