using InstaTransfer.ITExceptions.Updater;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using System;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Api
{
    public class CommandCreateDeclaration : Command
    {

        public CommandCreateDeclaration(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            try
            {

            }
            catch (FormatException e)
            {
                UException ex = new UException
                    (
                        GeneralErrors.FormatExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
                Logger.WriteErrorLog(ex, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }

        }
    }
}
