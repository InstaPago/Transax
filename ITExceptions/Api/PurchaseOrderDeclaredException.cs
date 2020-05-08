using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class PurchaseOrderDeclaredException : ITException
    {
        #region Constructors

        public PurchaseOrderDeclaredException(string message, Exception ex) : base(message, ex)
        {
        }
        public PurchaseOrderDeclaredException(string message) : base(message)
        {
        }
        public PurchaseOrderDeclaredException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PurchaseOrderDeclaredException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
