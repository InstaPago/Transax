using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaTransfer.BLL.Models.Email
{
    /// <summary>
    /// Modelo de correo electrónico
    /// </summary>
    public class EmailModels
    {
        /// <summary>
        /// Correo Remitente
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// Correo Receptores
        /// </summary>
        public List<string> To { get; set; }
        /// <summary>
        /// Asunto
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Nombre Remitente
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Cuerpo del correo
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Modelo del correo de solicitud de pago
        /// </summary>
        public class PaymentRequestEmailModel : EmailModels
        {
            /// <summary>
            /// Nombre del usuario pagador
            /// </summary>
            public string EndUserName { get; set; }
        }
    }
}
