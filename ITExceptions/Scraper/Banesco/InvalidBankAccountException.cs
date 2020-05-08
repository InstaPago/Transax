using System;

namespace InstaTransfer.ITExceptions.Scraper.Banesco
{
    public class InvalidBankAccountException : ITException
    {
        #region Constructors

        public InvalidBankAccountException(string message, Exception ex) : base(message, ex)
        {
        }
        public InvalidBankAccountException(string message) : base(message)
        {
        }
        public InvalidBankAccountException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public InvalidBankAccountException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
