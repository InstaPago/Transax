using InstaTransfer.BLL.Models.Commerce;
using InstaTransfer.BLL.Models.PaymentUser;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaymentRequest.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Cedula Requerida")]
        [Display(Name = "Cedula")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "Contraseña requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Debe especificar su Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe especificar su Contraseña")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debe confirmar su Contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        public DeclarationUserModel declarationUserModel { get; set; }

    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Cedula Requerida")]
        [Display(Name = "Cedula")]
        public string Cedula { get; set; }

        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Cedula Requerida")]
        [Display(Name = "Cedula")]
        public string Cedula { get; set; }

        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }
    }
}
