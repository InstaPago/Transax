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
    public class CUserBLL : Repository<CUser>
    {
        #region Read

        /// <summary>
        /// Retorna el usuario del comercio asociado al aspnet user logeado actualmente
        /// </summary>
        /// <param name="userID">Id del usuario asociado al usuario del comercio</param>
        /// <returns>El <see cref="CUser"/> del comercio asociado</returns>
        public CUser GetCUser(string userID)
        {
            return db.GetTable<CUser>().Where(cu => cu.IdAspNetUser == userID).FirstOrDefault();
        }

        /// <summary>
        /// Retorna todos los <see cref="CUser"/> asociados a un comercio especifico        
        /// </summary>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <returns>Lista de <see cref="CUser"/> del comercio asociado</returns>
        public List<CUser> GetCUsers(string rif)
        {
            return db.GetTable<CUser>().Where(cu => cu.RifCommerce == rif).ToList();
        }

        #endregion
    }
}
