using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class InsufficientFundsException : ITException
    {
        #region Constructors

        public InsufficientFundsException(string message, Exception ex) : base(message, ex)
        {
        }
        public InsufficientFundsException(string message) : base(message)
        {
        }
        public InsufficientFundsException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public InsufficientFundsException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
