using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Updater;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITLogic.Security;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Updater;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Updater
{
    public class CommandCreateBankStatement : Command
    {

        public CommandCreateBankStatement(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            Command commandSaveBankStatement;

            try
            {
                Dictionary<string, object> parameter = (Dictionary<string, object>)Parameter;
                string bankID = UpdaterHelper.GetBankIdString((Bank)Receiver);
                string socialReason = (string)parameter[GeneralResources.DictionaryKeySocialReason];
                DateTime date = DateTime.Parse((string)parameter[UpdaterResources.DictionaryKeyStatementDate]);


                Console.WriteLine("Creando estado de cuenta...");
                var bankStatement = new UBankStatement
                {
                    Date = date,
                    IdUBank_Receiver = bankID,
                    IdSocialReason = socialReason
                };
                //El Id del UBS esta dado por el hash de la combinacion entre la fecha y el Id del banco
                bankStatement.Id = ITSecurity.GetHashString(bankStatement.Date.ToString(UpdaterResources.ShortDateFormat)
                                           + bankStatement.IdUBank_Receiver + bankStatement.IdSocialReason);
                Receiver = bankStatement;
                //Guardamos el UBS creado en la base de datos
                commandSaveBankStatement = CommandFactory.GetCommandSaveBankStatement(bankStatement);
                commandSaveBankStatement.Execute();

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
