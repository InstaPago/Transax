using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITApi.Models
{
    /// <summary>
    /// Modelo de la respuesta POST de la orden de compra
    /// </summary>
    public class PurchaseOrderPostResponse
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
        /// Id de la orden
        /// </summary>
        public Guid idpurchaseorder { get; set; }
    }
}