using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Api
{
    public class CommandGetAllBankAccounts : Command
    {

        public CommandGetAllBankAccounts() : base() { }

        public override void Execute()
        {
            List<UBankAccount> bankAccounts; ;
            try
            {
                // Definimos el repositorio
                URepository<UBankAccount> UBARepo = new URepository<UBankAccount>();
                // Obtenemos todos los registros de la tabla
                bankAccounts = UBARepo.GetAllRecords().ToList();    
                // Guardamos el resultado en el Receiver
                Receiver = bankAccounts;
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
