using InstaTransfer.BLL.Models.PaymentUser;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.PurchaseOrder
{ 
    /// <summary>
    /// Modelo de creación de la orden de compra
    /// </summary>
    public class PurchaseOrderModel : BaseModel
    {
        /// <summary>
        /// Monto de la orden
        /// </summary>
        [Required(ErrorMessage = "Monto Requerido")]
        [Display(Name = "amount")]
        public decimal amount { get; set; }

        /// <summary>
        /// Número de la orden proporcionado por el comercio
        /// </summary>
        [Display(Name = "ordernumber")]
        public string ordernumber { get; set; }

        /// <summary>
        /// Rif del comercio
        /// </summary>
        [Display(Name = "rif")]
        public string rif { get; set; }

        /// <summary>
        /// Usuario pagador de la orden
        /// </summary>
        public PaymentUserModel paymentuser { get; set; }

        /// <summary>
        /// Constructor del modelo de la orden de compra
        /// </summary>
        public PurchaseOrderModel()
        {
            this.amount = 0;
            this.ordernumber = string.Empty;
            this.rif = string.Empty;
            this.paymentuser = new PaymentUserModel();
        }
    }

    /// <summary>
    /// Modelo del número de orden
    /// </summary>
    public class OrderNumberModel
    {
        /// <summary>
        /// Número de orden proporcionado por el comercio
        /// </summary>
        [Required]
        [Display(Name = "ordernumber")]
        public string ordernumber { get; set; }
    }
}