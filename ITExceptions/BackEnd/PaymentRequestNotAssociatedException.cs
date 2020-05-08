using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class PaymentRequestNotAssociatedException : ITException
    {
        #region Constructors

        public PaymentRequestNotAssociatedException(string message, Exception ex) : base(message, ex)
        {
        }
        public PaymentRequestNotAssociatedException(string message) : base(message)
        {
        }
        public PaymentRequestNotAssociatedException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PaymentRequestNotAssociatedException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
