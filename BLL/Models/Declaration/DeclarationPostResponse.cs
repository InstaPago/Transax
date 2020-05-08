using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.Declaration
{
    /// <summary>
    /// Modelo de la respuesta de la declaracion
    /// </summary>
    public class DeclarationPostResponse
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
        /// Id de la declaracion
        /// </summary>
        public Guid iddeclaration { get; set; }
    }
}