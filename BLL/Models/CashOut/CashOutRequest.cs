using InstaTransfer.BLL.Models.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace InstaTransfer.BLL.Models.CashOut
{
    /// <summary>
    /// Modelo de la solicitud de retiro
    /// </summary>
    public class CashOutRequest
    {
        [RequireNonDefault]
        /// <summary>
        /// Cuenta del comercio a asociar
        /// </summary>
        public Guid BankAccountId { get; set; }

        [Required]
        /// <summary>
        /// Monto a retirar
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Descripcion de retiro
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Constructor de una solicitud de retiro
        /// </summary>
        public CashOutRequest()
        {
            BankAccountId = new Guid();

            Amount = string.Empty;

            Description = string.Empty;
        }


    }
}