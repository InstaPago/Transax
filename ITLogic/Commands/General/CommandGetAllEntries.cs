using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.General
{
    public class CommandGetAllEntries : Command
    {

        public CommandGetAllEntries() : base() { }

        public override void Execute()
        {
            List<UBankStatementEntry> entries; ;
            try
            {
                Console.WriteLine("Obteniendo todos los usuarios activos...");
                // Definimos el repositorio
                URepository<UBankStatementEntry> UBSERepo = new URepository<UBankStatementEntry>();   

                switch ((BankStatementEntryType)Parameter)
                {
                    case BankStatementEntryType.Credit:
                        entries = UBSERepo.GetCredits().ToList();
                        break;
                    case BankStatementEntryType.Debit:
                        entries = UBSERepo.GetDebits().ToList();
                        break;
                    default:
                        entries = UBSERepo.GetAllRecords().ToList();
                        break;
                }
                //Obtenemos todos los registros que cumplen con los parametros
                Receiver = entries;
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
