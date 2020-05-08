using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.General
{
    public class CommandGetAllSocialReasons : Command
    {

        public CommandGetAllSocialReasons() : base() { }

        public override void Execute()
        {

            try
            {
                // Definimos el repositorio
                URepository<USocialReason> USRRepo = new URepository<USocialReason>();
                //Obtenemos todos los registros que cumplen con los parametros
                List<USocialReason> entries = USRRepo.GetAllRecords().ToList();
                Receiver = entries;
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
