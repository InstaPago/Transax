using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.PaymentUser
{
    /// <summary>
    /// Usuario pagador
    /// </summary>
    public class PaymentUserModel
    {
        /// <summary>
        /// Cédula de identidad del usuario pagador
        /// </summary>
        [Required(ErrorMessage = "Cedula de Identidad Requerida")]
        [Range(999999, 99999999, ErrorMessage = "La cedula debe comprender entre {1} y {2}")]
        [Display(Name = "userci")]
        public int userci { get; set; }

        /// <summary>
        /// Correo electrónico del usuario pagador
        /// </summary>
        [Required(ErrorMessage = "Correo Electronico Requerido")]
        [Display(Name = "useremail")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string useremail { get; set; }
    }

    public class DeclarationUserModel : PaymentUserModel
    {
        /// <summary>
        /// Nombre del Pagador
        /// </summary>
        [Required(ErrorMessage = "Nombre del declarante requerido")]
        [Display(Name = "userfirstname")]
        public string userfirstname { get; set; }

        /// <summary>
        /// Apellido del Pagador
        /// </summary>
        [Required(ErrorMessage = "Apellido del declarante requerido")]
        [Display(Name = "userlastname")]
        public string userlastname { get; set; }

        /// <summary>
        /// Teléfono del Pagador
        /// </summary>
        [Required(ErrorMessage = "Teléfono del declarante requerido")]
        [Display(Name = "userlastname")]
        public string userphone { get; set; }

        /// <summary>
        /// Contraseña de Pagador
        /// </summary>
        [Required(ErrorMessage = "Debe especificar su Contraseña")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string password { get; set; }

        [Required(ErrorMessage = "Debe confirmar su Contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "confirmpassword")]
        [Compare("password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string confirmpassword { get; set; }

    }

    public class DeclarationUserViewModel : PaymentUserModel
    {
        /// <summary>
        /// Nombre del Pagador
        /// </summary>
        [Display(Name = "userfirstname")]
        public string userfirstname { get; set; }

        /// <summary>
        /// Apellido del Pagador
        /// </summary>
        [Display(Name = "userlastname")]
        public string userlastname { get; set; }

        /// <summary>
        /// Apellido del Pagador
        /// </summary>
        [Display(Name = "userlastname")]
        public string userphone { get; set; }
    }
}