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
    /// <summary>
    /// Logica de negocio de las cuentas bancarias
    /// </summary>
    public class UBankAccountBLL : Repository<UBankAccount>
    {
        /// <summary>
        /// Obtiene la cuenta bancaria asociada a una cuenta pote y a un banco específico
        /// </summary>
        /// <param name="rif">Rif de la cuenta</param>
        /// <param name="bank">Banco de la cuenta</param>
        /// <returns>Cuenta bancaria asociada</returns>
        public UBankAccount GetBankAccount(string rif, string bankId)
        {
            var account = GetAllRecords(a => a.IdUSocialReason == rif && a.UBank.Id == bankId).FirstOrDefault();
            // Retornamos la cuenta
            return account;            
        }

      
    }
}
