using InstaTransfer.BLL.Models;
using InstaTransfer.BLL.Models.PaymentUser;
using InstaTransfer.BLL.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.Declaration
{
    /// <summary>
    /// Modelo de las declaraciones
    /// </summary>
    public class DeclarationModel : BaseModel
    {
        /// <summary>
        /// Identificador del banco emisor
        /// </summary>
        [Required(ErrorMessage = "Banco Emisor Requerido")]
        [StringLength(4, ErrorMessage = "El campo {0} debe tener {2} caracteres de longitud.", MinimumLength = 4)]
        [Display(Name = "idissuingbank")]
        public string idissuingbank { get; set; }

        /// <summary>
        /// Fecha en la que se realizó la transacción
        /// </summary>
        [Required(ErrorMessage = "Fecha Requerida")]
        [StringLength(8, ErrorMessage = "El campo {0} debe tener {2} caracteres de longitud.", MinimumLength = 8)]
        [DataType(DataType.Date)]
        [CustomDate]
        [Display(Name = "transactiondate")]
        public string transactiondate { get; set; }

        /// <summary>
        /// Identificador del tipo de operación
        /// </summary>
        [Required(ErrorMessage = "Tipo de Operacion Requerido")]
        [Display(Name = "idoperationtype")]
        public int idoperationtype { get; set; }

        /// <summary>
        /// Número de referencia de la transacción
        /// </summary>
        [Required(ErrorMessage = "Numero de Referencia Requerido")]
        [ReferenceValidation]
        [Display(Name = "referencenumber")]
        public string referencenumber { get; set; }

        /// <summary>
        /// Monto exacto de la transacción
        /// </summary>
        [Required(ErrorMessage = "Monto Requerido")]
        [Display(Name = "amount")]
        public decimal amount { get; set; }

        /// <summary>
        /// Identificador de la orden de compra asociada
        /// </summary>
        [Required]
        public Guid idpurchaseorder { get; set; }

        /// <summary>
        /// Usuario pagador de la declaración
        /// </summary>
        public PaymentUserModel paymentuser { get; set; }
    }

    public class DeclarationRequestModel : DeclarationModel
    {
        public DeclarationUserModel declarationuser { get; set; }

        public Guid idpaymentrequest { get; set; }

        [Required]
        public string requestemail { get; set; }

    }

    public class DeclarationRequestViewModel : DeclarationModel
    {
        public DeclarationUserViewModel declarationuser { get; set; }

        public Guid idpaymentrequest { get; set; }

        public string requestemail { get; set; }

    }
}