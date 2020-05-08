using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Log;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Updater
{
    public class CommandSaveEntries : Command
    {

        /// <summary>
        /// Guarda la lista de UBSE en la BD
        /// </summary>
        /// <param name="receiver">Lista de tipo <see cref="UBankStatementEntry"/> a almacenar</param>
        public CommandSaveEntries(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            List<UBankStatementEntry> bankStatementEntries = (List<UBankStatementEntry>)Receiver;
            try
            {
                Console.WriteLine("Almacenando movimientos bancarios...");

                // Definir repositorio
                URepository<UBankStatement> UBSERepo = new URepository<UBankStatement>();
                // Agregamos el registro creado a la tabla UBankStatement en la base de datos
                UBSERepo.BulkInsertAll(bankStatementEntries);
                // Guardamos los cambios
                UBSERepo.SaveChanges();
                Receiver = true;
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
