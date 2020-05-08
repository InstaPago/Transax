using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.Declaration
{
    /// <summary>
    /// Modelo de la respuesta de la declaracion
    /// </summary>
    public class DeclarationGetResponse
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
        /// Id del status de la declaracion
        /// </summary>
        public int idstatus { get; set; }
        /// <summary>
        /// Descripcion del status de la declaracion
        /// </summary>
        public string status { get; set; }
    }
}