using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITApi.Models
{
    /// <summary>
    /// Modelo de la respuesta del login
    /// </summary>
    public class LoginPostResponse
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
        /// Token JWT
        /// </summary>
        public string token { get; set; }
    }
}