using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class DeclarationAmountException : ITException
    {
        #region Constructors

        public DeclarationAmountException(string message, Exception ex) : base(message, ex)
        {
        }
        public DeclarationAmountException(string message) : base(message)
        {
        }
        public DeclarationAmountException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public DeclarationAmountException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
