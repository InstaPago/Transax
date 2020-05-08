using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class PaymentRequestProcessedException : ITException
    {
        #region Constructors

        public PaymentRequestProcessedException(string message, Exception ex) : base(message, ex)
        {
        }
        public PaymentRequestProcessedException(string message) : base(message)
        {
        }
        public PaymentRequestProcessedException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PaymentRequestProcessedException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion
    }
}
