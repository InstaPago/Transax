using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ITApi.Models
{
    /// <summary>
    /// Modelo de inicio de sesión
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string email { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
    }
}