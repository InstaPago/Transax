using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MyHelpers.StringExtensions
{
    public static class StringExtensions
    {
        public static int ToInt32(this string cadena)
        {
            return Convert.ToInt32(cadena);
        }

        public static string prueba()
        {

            return "prueba";
        }
        public static bool IsNullOrEmpty(this string cadena)
        {
            return String.IsNullOrEmpty(cadena);
        }

        public static string CropText(this String Text, int Size)
        {
            if (Text.Length > Size)
                return (Text.Substring(0, Size) + "...");
            else
                return Text;
        }

        public static string CropTextWithHTML(this string text, int size)
        {
            StringBuilder s = new StringBuilder(text);
            bool inHtml = false;
            bool removeText = false;
            int contText = 0;
            int indiceInsert = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '<')
                    inHtml = true;

                if (!inHtml)
                {
                    contText++;
                    if (contText == size)
                    {
                        removeText = true;
                        indiceInsert = i;
                    }
                    if (removeText)
                    {
                        s.Remove(i, 1);
                        i--;
                    }
                }

                if (s[i] == '>')
                    inHtml = false;
            }
            if (removeText)
                s.Insert(indiceInsert, "...");
            return s.ToString();
        }

        /// <summary>
        /// Remueve todo los tags html que se encuentren en un texto
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveHTML(this string text)
        {
            string HTML_TAG_PATTERN = "<.*?>";
            return Regex.Replace
              (text, HTML_TAG_PATTERN, string.Empty);
        }

        public static string EncryptKey(this string cadena, string CryptoKey)
        {

            byte[] keyArray;

            byte[] encodeArray = UTF8Encoding.UTF8.GetBytes(cadena);

            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(CryptoKey));

            hashmd5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;


            ICryptoTransform cTransform = tdes.CreateEncryptor();

            byte[] ArrayResultado = cTransform.TransformFinalBlock(encodeArray, 0, encodeArray.Length);

            tdes.Clear();

            return Convert.ToBase64String(ArrayResultado, 0, ArrayResultado.Length);

        }

        public static string DecryptKey(this string clave, string CryptoKey)
        {
            byte[] keyArray;

            byte[] decodeArray = Convert.FromBase64String(clave);


            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(CryptoKey));

            hashmd5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();

            byte[] resultArray =
            cTransform.TransformFinalBlock(decodeArray, 0, decodeArray.Length);

            tdes.Clear();

            return UTF8Encoding.UTF8.GetString(resultArray);

        }

    }
}
