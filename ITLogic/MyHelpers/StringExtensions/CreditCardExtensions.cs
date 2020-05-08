using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyHelpers.StringExtensions
{
    public static class CreditCardExtensions
    {
        /// <summary>
        /// Obtiene los ultimos 4 digitos de un numero de tarjeta
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Los ultimos 4 digitos</returns>
        public static string GetLast4Digits(string number)
        {
            return number.Substring(number.Length - 4);
        }

        /// <summary>
        /// Verifica si el numero de la tarjeta es Visa
        /// 
        /// Para mas informacion:
        /// http://stackoverflow.com/questions/72768/how-do-you-detect-credit-card-type-based-on-number
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es Visa</returns>
        public static bool IsVisa(string number)
        {
            const string pattern = @"^4[0-9]{12}(?:[0-9]{3})?$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }

        /// <summary>
        /// Verifica si el numero de la tarjeta es MasterCard
        /// 
        /// Para mas informacion:
        /// https://github.com/bendrucker/creditcards-types/blob/master/src/types.js
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es MasterCard</returns>
        public static bool IsMasterCard(string number)
        {
            //const string pattern = @"^2[0-9][0-9]{14}$|^5[0-9][0-9]{14}$";
            const string pattern = @"^(5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)\d{12}$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }

        /// <summary>
        /// Verifica si el numero de la tarjeta es American Express
        /// 
        /// Para mas informacion:
        /// https://github.com/bendrucker/creditcards-types/blob/master/src/types.js
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es American Express</returns>
        public static bool IsAmericanExpress(string number)
        {
            const string pattern = @"^3[47]\d{13}$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }

        /// <summary>
        /// Verifica si el numero de la tarjeta es Diners Club
        /// 
        /// Para mas informacion:
        /// https://github.com/bendrucker/creditcards-types/blob/master/src/types.js
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es Diners</returns>
        public static bool IsDinersClub(string number)
        {
            const string pattern = @"^3(0[0-5]|[68]\d)\d{11}$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }

        /// <summary>
        /// Verifica si el numero de la tarjeta es Discover
        /// 
        /// Para mas informacion:
        /// http://stackoverflow.com/questions/72768/how-do-you-detect-credit-card-type-based-on-number
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es Discover</returns>
        public static bool IsDiscover(string number)
        {
            const string pattern = @"^6(011(0[0-9]|[2-4]\d|74|7[7-9]|8[6-9]|9[0-9])|4[4-9]\d{3}|5\d{4})\d{10}$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }

        /// <summary>
        /// Verifica si el numero de la tarjeta es JCB
        /// 
        /// Para mas informacion:
        /// http://stackoverflow.com/questions/72768/how-do-you-detect-credit-card-type-based-on-number
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es JCB</returns>
        public static bool IsJCB(string number)
        {
            const string pattern = @"^(?:2131|1800|35\d{3})\d{11}$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }


        /// <summary>
        /// Verifica si el numero de la tarjeta es MAESTRO
        /// 
        /// Para mas informacion:
        /// https://github.com/bendrucker/creditcards-types/blob/master/src/types.js
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es MAESTRO</returns>
        public static bool IsMaestro(string number)
        {
            const string pattern = @"^(?:5[06789]\d\d|(?!6011[0234])(?!60117[4789])(?!60118[6789])(?!60119)(?!64[456789])(?!65)6\d{3})\d{8,15}$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }

        /// <summary>
        /// Verifica si el numero de la tarjeta es valida, sin determinar la marca
        /// 
        /// Para mas informacion:
        /// http://stackoverflow.com/questions/72768/how-do-you-detect-credit-card-type-based-on-number
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si es valida</returns>
        public static bool IsValid(string number)
        {
            //const string pattern = @"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$";
            const string pattern = @"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$";
            var match = System.Text.RegularExpressions.Regex.IsMatch(number, pattern);
            return match;
        }

        /// <summary>
        /// Verifica si un numero de tarjeta pasa la prueba de Luhn
        /// 
        /// Para mas informacion:
        /// http://www.codeproject.com/Articles/20271/Ultimate-NET-Credit-Card-Utility-Class
        /// </summary>
        /// <param name="number">El numero de tarjeta</param>
        /// <returns>Si pasa la prueba</returns>
        public static bool PassesLuhnTest(string number)
        {
            // Convert card number into digits array
            var digits = new int[number.Length];
            for (var length = 0; length < number.Length; length++)
            {
                digits[length] = Int32.Parse(number.Substring(length, 1));
            }

            // Luhn Algorithm
            // Adapted from code availabe on Wikipedia at
            // http://en.wikipedia.org/wiki/Luhn_algorithm
            var sum = 0;
            var alt = false;
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var currentDigit = digits[i];
                if (alt)
                {
                    currentDigit *= 2;
                    if (currentDigit > 9)
                    {
                        currentDigit -= 9;
                    }
                }
                sum += currentDigit;
                alt = !alt;
            }

            // If mod 10 equals 0, the number is good and this will return true
            return sum % 10 == 0;
        }

        public static string getCardType(string cardNumber)
        {
            if (IsAmericanExpress(cardNumber))
            {
                return "American Express";
            }

            if (IsMasterCard(cardNumber))
            {
                return "MasterCard";
            }

            if (IsVisa(cardNumber))
            {
                return "Visa";
            }
            if (cardNumber.Substring(0, 6).Equals("824400"))
            {
                return "Sambil";
            }
            if (cardNumber.Substring(0, 6).Equals("824404"))
            {
                return "Locatel";
            }
            if (IsMaestro(cardNumber))
            {
                return "Maestro";
            }


            return "Invalid";
        }

        /// <summary>
        /// Limpia el numero de espacios en blanco y guiones
        /// </summary>
        /// <param name="number">El numero</param>
        /// <returns>El numero</returns>
        public static string CleanNumber(string number)
        {
            if (number.Contains(" "))
                number = number.Replace(" ", "");

            if (number.Contains("-"))
                number = number.Replace("-", "");

            return number;
        }

        public static string TransformesCard(string created, string id, string idafiliado, string cardnumber)
        {
            string _CardNumber = "";
            string key = "";
            byte[] keyArray;
            key = idafiliado + created + id;
            key = key.ToLower();
            byte[] Arreglo_a_Cifrar = UTF8Encoding.UTF8.GetBytes(cardnumber);
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] ArrayResultado = cTransform.TransformFinalBlock(Arreglo_a_Cifrar, 0, Arreglo_a_Cifrar.Length);
            tdes.Clear();
            return _CardNumber = Convert.ToBase64String(ArrayResultado, 0, ArrayResultado.Length);
        }

        public static string DesTransformesCard(string created, string id, string idafiliado, string cardnumber)
        {
            string key = "";
            byte[] keyArray;
            key = idafiliado + created + id;
            key = key.ToLower();
            byte[] Array_a_Descifrar = Convert.FromBase64String(cardnumber);
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(Array_a_Descifrar, 0, Array_a_Descifrar.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static bool IsAcceptedCard(this string CardNumber)
        {
            bool ret = false;
            switch (CardNumber.Substring(0, 1))
            {
                //MASTERCARD
                case "2":
                    ret = true;
                    break;
                //AMEX
                case "3":
                    ret = true;
                    break;
                //VISA
                case "4":
                    ret = true;
                    break;
                //MASTERCARD
                case "5":
                    ret = true;
                    break;
                //SAMBIL & LOCATEL
                case "8":
                    ret = true;
                    break;
            }

            return ret;
        }
    }
}
