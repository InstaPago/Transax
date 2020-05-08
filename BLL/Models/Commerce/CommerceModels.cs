using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.Commerce
{
    public class RegisterCommerceModel
    {
        [Required(ErrorMessage = "Debe especificar el RIF de su empresa")]
        public string Rif { get; set; }
        [Required(ErrorMessage = "Debe especificar su Nombre Comercial")]
        public string BusinessName { get; set; }
        [Required(ErrorMessage = "Debe especificar su Razón Social")]
        public string SocialReasonName { get; set; }
        [Required(ErrorMessage = "Debe especificar su Dirección")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Debe especificar el Telefono")]
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        public RegisterCommerceModel()
        {
            this.Rif = string.Empty;
            this.BusinessName = string.Empty;
            this.SocialReasonName = string.Empty;
            this.Address = string.Empty;
            this.Phone = string.Empty;
        }
    }

    public class ModifyCommerceModel
    {
        [Required(ErrorMessage = "Debe especificar el RIF de su empresa")]
        public string Rif { get; set; }
        [Required(ErrorMessage = "Debe especificar su Nombre Comercial")]
        public string BusinessName { get; set; }
        [Required(ErrorMessage = "Debe especificar su Razón Social")]
        public string SocialReasonName { get; set; }
        [Required(ErrorMessage = "Debe especificar su Dirección")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Debe especificar el Telefono")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Debe especificar la Comisión")]
        public string WithdrawalFee { get; set; }
        [Required(ErrorMessage = "Debe especificar el factor de riesgo")]
        public string Trust { get; set; }

        public ModifyCommerceModel()
        {
            this.Rif = string.Empty;
            this.BusinessName = string.Empty;
            this.SocialReasonName = string.Empty;
            this.Address = string.Empty;
            this.Phone = string.Empty;
            this.WithdrawalFee = string.Empty;
            this.Trust = string.Empty;
        }
    }
}