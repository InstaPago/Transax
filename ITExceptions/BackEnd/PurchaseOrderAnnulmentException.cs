using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class PurchaseOrderAnnulmentException : ITException
    {
        #region Constructors

        public PurchaseOrderAnnulmentException(string message, Exception ex) : base(message, ex)
        {
        }
        public PurchaseOrderAnnulmentException(string message) : base(message)
        {
        }
        public PurchaseOrderAnnulmentException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PurchaseOrderAnnulmentException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
