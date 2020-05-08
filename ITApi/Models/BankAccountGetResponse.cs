using InstaTransfer.BLL.Models.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITApi.Models
{
    /// <summary>
    /// Modelo de la respuesta GET de las cuentas bancarias
    /// </summary>
    public class BankAccountGetResponse
    {
        /// <summary>
        /// Estado de la operacion
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// Mensaje de la operacion
        /// </summary>
        public List<string> message { get; set; }
        /// <summary>
        /// Listado de cuentas bancarias
        /// </summary>
        public List<BankAccountModel> bankAccounts { get; set; }

    }
}