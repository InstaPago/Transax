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
    public class CommerceBLL : Repository<Commerce>
    {
        /// <summary>
        /// Retorna el comercio asociado a una declaracion especifica
        /// </summary>
        /// <param name="idDeclaration">Id de la declaracion</param>
        /// <returns>Comercio asociado a la declaracion</returns>
        public Commerce GetCommerce(Guid idDeclaration)
        {
            Commerce commerce;
            using (var DBLL = new DeclarationBLL())
            {
                commerce = DBLL.GetDeclaration(idDeclaration).Commerce;
            }
            return commerce;
        }

        /// <summary>
        /// Retorna el comercio asociado a un rif especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <returns>Comercio asociado al rif especificado</returns>
        public Commerce GetCommerce(string rif)
        {
            return GetAllRecords(c => c.Rif == rif).FirstOrDefault();
        }
    }
}
