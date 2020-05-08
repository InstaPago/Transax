using InstaTransfer.BLL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PaymentRequest.Models
{
    /// <summary>
    /// Modelo de modificacion de los usuarios
    /// </summary>
    public class ModifyEndUserModel : BaseModel
    {
        /// <summary>
        /// Nombre del Usuario
        /// </summary>
        [StringLength(30, ErrorMessage = "El {0} no debe exceder {1} caracteres de longitud.")]
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Debe especificar su Nombre")]
        public string Name { get; set; }
        /// <summary>
        /// Apellido del Usuario
        /// </summary>
        [StringLength(30, ErrorMessage = "El {0} no debe exceder {1} caracteres de longitud.")]
        [Display(Name = "Apellido")]
        [Required(ErrorMessage = "Debe especificar su Apellido")]
        public string LastName { get; set; }
        /// <summary>
        /// Correo del Usuario
        /// </summary>
        [EmailAddress(ErrorMessage = "{0} inválido")]
        [StringLength(50, ErrorMessage = "El {0} no debe exceder {1} caracteres de longitud.")]
        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "Debe especificar el Correo")]
        public string Email { get; set; }
        /// <summary>
        /// Teléfono del Usuario
        /// </summary>
        [StringLength(20, ErrorMessage = "El {0} no debe exceder {1} caracteres de longitud.")]
        [Display(Name = "Teléfono")]
        [Required(ErrorMessage = "Debe especificar el Teléfono")]
        public string Phone { get; set; }

        public ModifyEndUserModel()
        {
            Name = string.Empty;
            LastName = string.Empty;            
            Email = string.Empty;
            Phone = string.Empty;
        }
    }
}