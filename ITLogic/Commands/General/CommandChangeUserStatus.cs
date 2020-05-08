using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.General
{
    public class CommandChangeUserStatus : Command
    {

        public CommandChangeUserStatus(Object receiver) : base(receiver) { }

        public override void Execute()
        {

            try
            {
                Console.WriteLine("Cambiando el estado del usuario...");

                UUser currentUser = (UUser)Receiver;
                //Guardo los parametros desde el diccionario
                Dictionary<string, object> parameters = (Dictionary<string, object>)Parameter;
                UmbrellaUserStatus status = (UmbrellaUserStatus)parameters[GeneralResources.DictionaryKeyUserStatus];

                // Definimos el repositorio
                URepository<UUser> UUserRepo = new URepository<UUser>();
                // Actualizamos el usuario con los parametros deseados
                var updatedUser = UUserRepo.ChangeUmbrellaUserStatus(currentUser.Username, currentUser.IdUBank, Convert.ToInt32(status));
                //Guardamos el usuario en el receiver
                Receiver = updatedUser;
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }

        }
    }
}
