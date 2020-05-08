using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.General
{
    public class CommandGetUser : Command
    {

        public CommandGetUser() : base() { }

        public override void Execute()
        {

            try
            {
                Console.WriteLine("Obteniendo usuario de banca en linea asociado...");

                //Guardo los parametros desde el diccionario
                Dictionary<string, string> parameters = (Dictionary<string, string>)Parameter;
                string username = parameters[GeneralResources.DictionaryKeyUsername];
                string bankID = parameters[GeneralResources.DictionaryKeyBank];

                // Definimos el repositorio
                URepository<UUser> UUserRepo = new URepository<UUser>();
                //Obtenemos todos los registros que cumplen con los parametros
                UUser user = UUserRepo.GetAllRecords(u => u.Username == username && u.IdUBank == bankID).First();
                Receiver = user;
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }

        }
    }
}
