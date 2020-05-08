using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class DeclarationPaymentUserException : ITException
    {
        #region Constructors

        public DeclarationPaymentUserException(string message, Exception ex) : base(message, ex)
        {
        }
        public DeclarationPaymentUserException(string message) : base(message)
        {
        }
        public DeclarationPaymentUserException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public DeclarationPaymentUserException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
