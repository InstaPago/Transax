using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models.Validation
{
    /// <summary>
    /// Atributo de validacion del formato de la fecha
    /// </summary>
    public class CustomDateAttribute : ValidationAttribute
    {
        public string Format { get; set; }
        public DateTime MaxDate { get; set; }

        /// <summary>
        /// Constructor del atributo
        /// </summary>
        public CustomDateAttribute()
        {
            this.Format = "dd/MM/yy";
            this.MaxDate = DateTime.Now;
        }

        /// <summary>
        /// Verifica si el atributo es valido
        /// </summary>
        /// <param name="value">Valor a validar</param>
        /// <param name="validationContext">Contexto de validacion</param>
        /// <returns>Resultado de la validacion</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (DateTime.ParseExact((string)value, Format, CultureInfo.GetCultureInfo("es-VE")) > MaxDate)
                {
                    return new ValidationResult("La fecha debe ser menor a la fecha actual", new[] { validationContext.MemberName });
                }
            }
            catch (FormatException)
            {
                return new ValidationResult("Formato de fecha errado", new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
