using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyHelpers.Utilities
{
    public static class RegexPatterns
    {
        public const string DateTimeRegexPatternES = @"^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((19|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((19|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((19|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$";
        public const string UriRegexPattern = @"((mailto\:|(news|(ht|f)tp(s?))\://){1}\S+)";
        public const string EmailRegexPattern = @"^[\w-]+(\.[\w-]+)*@([a-z0-9-]+(\.[a-z0-9-]+)*?\.[a-z]{2,6}|(\d{1,3}\.){3}\d{1,3})(:\d{4})?$";
        public const string TimeRegexPattern = @"^((([0]?[1-9]|1[0-2])(:|\.)[0-5][0-9]((:|\.)[0-5][0-9])?( )?(AM|am|aM|Am|PM|pm|pM|Pm))|(([0]?[0-9]|1[0-9]|2[0-3])(:|\.)[0-5][0-9]((:|\.)[0-5][0-9])?))$";
        public const string PointRegexPattern = @"^[-+]?\d+(\.\d+)?$";
        public const string CharactersAndNumbersRegexPattern = @"^\s*[a-zA-Z0-9,\s]+\s*$";
        public const string TwitterRegexPattern = @"^(@[a-zA-Z0-9]{1,15})$";
        public const string CreditCardRegexPattern = @"^((4\d{3})|(5[1-5]\d{2})|(6011))-?\d{4}-?\d{4}-?\d{4}|3[4,7]\d{13}$";
        public const string CVVRegexPattern = @"^([0-9]{3,4})$";
        public const string Cedula_Rif = @"^(\d+)$";
        public const string NumberCard = @"^([0-9]{15,16})$";
        public const string DigitsRegexPattern = @"^(\d+)$";
        public const string ExpirationDatePattern = @"^(([0][0-9])||([1][0-2]))\/(\d{4})$";
        public const string NotDigitsRegexPattern = @"^(\D+)$";
        public const string Rif = @"^[v|V|e|E|j|J|g|G]-\d{8}-?\d$";
        public const string RifNumber = @"^\d{8}-?\d$";
        public const string Phone = @"(((0|\+)\d{2})|0)[\d]{3}-?[\d]{7,12}";
        public const string OnlyNumbersRegexPattern = @"^[0-9]+$";
        public const string OnlyLettersRegexPattern = @"^[a-zA-Z]+$";
        public const string OnlyLettersAndSpacesRegexPattern = "^[A-Za-z ]+$";
    }
}
