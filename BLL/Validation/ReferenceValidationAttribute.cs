using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace InstaTransfer.BLL.Models.Validation
{
    /// <summary>
    /// Atributo de validacion del numero de referencia
    /// </summary>
    public class ReferenceValidationAttribute : ValidationAttribute
    {
        ValidationAttribute noWhiteSpace = new NoWhiteSpaceAttribute();

        /// <summary>
        /// Verifica si el atributo es valido
        /// </summary>
        /// <param name="value">Valor a validar</param>
        /// <param name="validationContext">Contexto de validacion</param>
        /// <returns>Resultado de la validacion</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringValue = value as string;
            var nwsResult = noWhiteSpace.GetValidationResult(value, validationContext);
            if (nwsResult != ValidationResult.Success)
            {
                return nwsResult;
            }
            else if (!stringValue.All(char.IsLetterOrDigit))
            {
                return new ValidationResult(string.Format("El valor de {0} solo puede contener numeros o letras", validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }


    }
}