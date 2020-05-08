using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class PaymentRequestAnnulledException : ITException
    {
        #region Constructors

        public PaymentRequestAnnulledException(string message, Exception ex) : base(message, ex)
        {
        }
        public PaymentRequestAnnulledException(string message) : base(message)
        {
        }
        public PaymentRequestAnnulledException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PaymentRequestAnnulledException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion
    }
}
