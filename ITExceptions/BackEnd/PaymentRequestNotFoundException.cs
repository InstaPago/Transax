using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class PaymentRequestNotFoundException : ITException
    {
        #region Constructors

        public PaymentRequestNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }
        public PaymentRequestNotFoundException(string message) : base(message)
        {
        }
        public PaymentRequestNotFoundException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PaymentRequestNotFoundException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
