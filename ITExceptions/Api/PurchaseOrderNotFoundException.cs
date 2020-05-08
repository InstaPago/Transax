using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class PurchaseOrderNotFoundException : ITException
    {
        #region Constructors

        public PurchaseOrderNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }
        public PurchaseOrderNotFoundException(string message) : base(message)
        {
        }
        public PurchaseOrderNotFoundException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PurchaseOrderNotFoundException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
