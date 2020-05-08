using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MyHelpers.Utilities
{
    public class Util
    {
        /// <summary>
        /// Devuelve un TimeStamp
        /// </summary>
        /// <returns></returns>
        public static String GetTimeStamp()
        {
            String ret = String.Empty;
            DateTime t = System.DateTime.Today;

            ret = t.Year.ToString() + t.Month.ToString() + t.Day.ToString() + t.Hour.ToString() + t.Minute.ToString();
            return ret;
        }

        /// <summary>
        /// Corta un texto y le pone "..." al final si es mas grande
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Size"></param>
        /// <returns></returns>
        public static String CropText(String Text, int Size)
        {
            if (Text.Length > Size)
                return (Text.Substring(0, Size) + "...");
            else
                return Text;
        }

        /// <summary>
        /// Retorna el id del idioma en el que se esta trabajando actualmente. Retorna 1 si es español y 2 si es ingles.
        /// En caso de agregar mas idiomas a la aplicacion se debe modificar esta función
        /// </summary>
        /// <returns></returns>
        public static int GetIdIdioma()
        {
            int idIdioma = (Thread.CurrentThread.CurrentCulture.Name.Contains("es"))? 1 : 2;
            return idIdioma;
        }

        public static String getInMD5(String inputString)
        {
            byte[] input = Encoding.UTF8.GetBytes(inputString);
            byte[] output = MD5.Create().ComputeHash(input);
            var sb = new StringBuilder(output.Length);
            for (int i = 0; i < output.Length; i++)
            {
                sb.Append(output[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

    }
}
