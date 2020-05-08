using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.General
{
    public class CommandGetAllUsers : Command
    {

        public CommandGetAllUsers() : base() { }

        public override void Execute()
        {

            try
            {
                // Definimos el repositorio
                URepository<UUser> UUserRepo = new URepository<UUser>();
                //Obtenemos todos los registros que cumplen con los parametros
                List<UUser> users = UUserRepo.GetAllActiveUmbrellaUsers().ToList();
                Receiver = users;
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, "Scraper (CommandGetAllUsers)");
            }
        }
    }
}
