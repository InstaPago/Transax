using System;
using System.ComponentModel.DataAnnotations;

namespace InstaTransfer.BLL.Models.PaymentRequest
{
    /// <summary>
    /// Modelo de la solicitud de pago
    /// </summary>
    public class PaymentRequestModel
    {
        [Required]
        /// <summary>
        /// Cedula del pagador
        /// </summary>
        public string UserCI { get; set; }
        [Required]
        /// <summary>
        /// Nombre del pagador
        /// </summary>
        public string UserName { get; set; }
        [Required]
        /// <summary>
        /// Apellido del pagador
        /// </summary>
        public string UserLastName { get; set; }
        [Required]
        /// <summary>
        /// Correo de contacto del pagador
        /// </summary>
        public string RequestEmail { get; set; }
        [Required]
        /// <summary>
        /// Telefono del pagador
        /// </summary>
        public string UserPhone { get; set; }
        [Required]
        /// <summary>
        /// Monto a pagar
        /// </summary>
        public string Amount { get; set; }
        [Required]
        /// <summary>
        /// Descripcion de la solicitud
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Constructor de una solicitud de pago
        /// </summary>
        public PaymentRequestModel()
        {
            UserCI = string.Empty;

            UserName = string.Empty;

            UserLastName = string.Empty;

            RequestEmail = string.Empty;

            UserPhone = string.Empty;

            Amount = string.Empty;

            Description = string.Empty;
        }
    }
}