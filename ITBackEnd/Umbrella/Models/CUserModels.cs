using InstaTransfer.BLL.Models;
using InstaTransfer.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Umbrella.Models
{
    public class RegisterCommerceUserModel
    {
        [Required(ErrorMessage = "Debe especificar su Nombre")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Debe especificar su Apellido")]
        public string LastName { get; set; }
        public string Role { get; set; }
        [Required(ErrorMessage = "Debe especificar el Modo de Acceso")]
        public bool TestMode { get; set; }

        public RegisterCommerceUserModel()
        {
            this.Name = string.Empty;
            this.LastName = string.Empty;
            this.Role = string.Empty;
        }
    }

    public class ModifyCUserModel : BaseModel
    {
        public string Email { get; set; }
        [Required(ErrorMessage = "Debe especificar su Nombre")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Debe especificar su Apellido")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Debe especificar el Rol")]
        public string RoleId { get; set; }
        public CUserStatus Status { get; set; }
        [Required(ErrorMessage = "Debe especificar el Modo de Acceso")]
        public bool TestMode { get; set; }
        [Required(ErrorMessage = "Debe especificar el Modo de Contacto")]
        public bool IsContact { get; set; }
        

        public ModifyCUserModel()
        {
            this.Email = string.Empty;
            this.Name = string.Empty;
            this.LastName = string.Empty;
            this.RoleId = string.Empty;
        }
    }
}