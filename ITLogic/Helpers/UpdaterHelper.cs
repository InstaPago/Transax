using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Updater;
using System;
using System.Globalization;
using System.Linq;

namespace InstaTransfer.ITLogic.Helpers
{
    public class UpdaterHelper
    {
        /// <summary>
        /// Establece la cultura por defecto de la aplicación
        /// </summary>
        public static void SetCulture(string defaultCulture)
        {
            CultureInfo culture = new CultureInfo(defaultCulture);
            CultureInfo.DefaultThreadCurrentCulture = culture;
        }

        /// <summary>
        /// Elimina o reemplaza los caracteres especificados del valor ingresado
        /// </summary>
        /// <param name="value">Valor a limpiar</param>
        /// <param name="type">Tipo de operacion de limpieza</param>
        /// <returns></returns>
        public static string CleanValues(string value, string type)
        {
            string cleanString = value;

            if (type == UpdaterResources.CleanValueQuotes)
            {
                cleanString = value.Replace("\"", "").Trim();
            }
            else if (type == UpdaterResources.CleanValueDate)
            {
                cleanString = value.Replace(@"-", @"/").Replace("\"", "").Trim();
            }
            return cleanString;
        }

        /// <summary>
        /// Devuelve un arreglo de todos los valores de tipo int para el enum <see cref="Bank"/>
        /// </summary>
        /// <returns>Arreglo de int de los valores de los enum <see cref="Bank"/></returns>
        public static string[] GetBankIdStringArray()
        {
            var _valueList = Enum.GetValues(typeof(Bank))
                                 .Cast<int>()
                                 .Select(x => x.ToString("0000"))
                                 .ToArray();
            return _valueList;
        }

        /// <summary>
        /// Convierte un enum de tipo <see cref="Bank"/> a un string en formato "0000"
        /// </summary>
        /// <param name="bankId">El Enum de tipo <see cref="Bank"/> a convertir</param>
        /// <returns>La representacion en formato string del Enum <see cref="Bank"/></returns>
        public static string GetBankIdString(Bank bank)
        {
            string _bankIdString = ((int)bank).ToString("0000");
            return _bankIdString;
        }

    }
}
