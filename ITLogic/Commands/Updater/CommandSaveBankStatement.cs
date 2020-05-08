using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Log;
using System;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Updater
{
    public class CommandSaveBankStatement : Command
    {

        /// <summary>
        /// Guarda el UBS generado en la BD
        /// </summary>
        /// <param name="receiver"><see cref="UBankStatement"/> a alamcenar</param>
        public CommandSaveBankStatement(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            try
            {
                Console.WriteLine("Almacenando estado de cuenta...");

                // Definir repositorio
                URepository<UBankStatement> UBSRepo = new URepository<UBankStatement>();
                // Agregamos el registro creado a la tabla UBankStatement en la base de datos
                UBSRepo.AddEntity((UBankStatement)Receiver);
                //AddStatementToBank(_uBankStatement);
                // Guardamos los cambios
                UBSRepo.SaveChanges();
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
