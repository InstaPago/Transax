using InstaTransfer.BLL.Models.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITApi.Models
{
    /// <summary>
    /// Modelo de la respuesta GET de la orden de compra
    /// </summary>
    public class PurchaseOrderGetResponse
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
        /// Orden de compra
        /// </summary>
        public PurchaseOrderViewModel purchaseorder { get; set; }

    }
}