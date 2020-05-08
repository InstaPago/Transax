using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.PurchaseOrder
{
    /// <summary>
    /// Modelo de consulta de órdenes de compra
    /// </summary>
    public class PurchaseOrderViewModel : PurchaseOrderModel
    {
        /// <summary>
        /// Fecha de creación de la orden de compra
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime creationdate { get; set; }       
    }
}