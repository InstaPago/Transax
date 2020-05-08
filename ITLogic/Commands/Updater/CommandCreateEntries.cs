using InstaTransfer.ITExceptions.Updater;
using InstaTransfer.ITResources.General;
using System;
using System.IO;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Updater
{
    public class CommandCreateEntries : Command
    {

        public CommandCreateEntries(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            try
            {
               
            }
            catch (IOException e)
            {
                throw new UException
                    (
                        GeneralErrors.IOExceptionCode,
                        typeof(CommandCreateEntries).Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (FormatException e)
            {
                throw new UException
                    (
                        GeneralErrors.IOExceptionCode,
                        typeof(CommandCreateEntries).Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
