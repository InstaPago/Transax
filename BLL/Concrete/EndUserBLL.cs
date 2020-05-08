using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Concrete
{
    public class EndUserBLL : Repository<EndUser>
    {
        /// <summary>
        /// Retorna el usuario final asociado al aspnet user logeado actualmente
        /// </summary>
        /// <param name="userID">Id del usuario asociado al usuario final</param>
        /// <returns>El usuario final</returns>
        public EndUser GetEndUser(string userID)
        {
            return db.GetTable<EndUser>().Where(eu => eu.IdAspNetUser == userID).FirstOrDefault();
        }

        /// <summary>
        /// Retorna el usuario por la cedula asociada
        /// </summary>
        /// <param name="CI">Cedula del usuario</param>
        /// <returns>Usuario pagador</returns>
        public EndUser GetEndUserByCI(string CI)
        {
            return db.GetTable<EndUser>().Where(eu => eu.CI == CI).FirstOrDefault();
        }

        /// <summary>
        /// Asocia un usuario final a un usuario aspnet
        /// </summary>
        /// <param name="userID">id del usuario aspnet</param>
        /// <returns></returns>
        public EndUser Register(string endUserCI, string aspUserID)
        {
            // Variables
            var aspUser = new AspNetUser();
            var endUser = new EndUser();

            // Obtenemos los usuarios
            endUser = GetAllRecords(eu => eu.CI.Equals(endUserCI)).FirstOrDefault();

            // Asociamos los usuarios
            endUser.IdAspNetUser = aspUserID;

            // Guardamos los cambios
            SaveChanges();

            // Retornamos el usuario
            return endUser;
        }
    }
}
