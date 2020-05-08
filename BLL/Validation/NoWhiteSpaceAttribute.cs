using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace InstaTransfer.BLL.Models.Validation
{
    /// <summary>
    /// Atributo de validacion de strings sin espacios en blanco
    /// </summary>
    public class NoWhiteSpaceAttribute : ValidationAttribute
    {
        Regex NoWSRegex = new Regex(@"^(?:\w+\s?)+\w+$");

        /// <summary>
        /// Verifica si el atributo es valido
        /// </summary>
        /// <param name="value">Valor a validar</param>
        /// <param name="validationContext">Contexto de validacion</param>
        /// <returns>Resultado de la validacion</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
                var stringValue = value as string;  

                if (!NoWSRegex.IsMatch(stringValue))
                {
                    return new ValidationResult(string.Format("El valor del {0} no debe contener espacios en blanco al inicio y fin", validationContext.DisplayName));
                }

            return ValidationResult.Success;
        }
    }
}