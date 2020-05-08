using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ITApi.Models
{
    public class BankAccountModel
    {
        public string accountnumber { get; set; }

        public string bankreceiver { get; set; }

        public string rif { get; set; }

        public string[] bankinstructions { get; set; }
    }

    public class IssuingBankModel
    {
        public string id { get; set; }

        public string bankname { get; set; }
    }
}