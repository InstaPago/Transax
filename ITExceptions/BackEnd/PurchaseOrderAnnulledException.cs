using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class PurchaseOrderAnnulledException : ITException
    {
        #region Constructors

        public PurchaseOrderAnnulledException(string message, Exception ex) : base(message, ex)
        {
        }
        public PurchaseOrderAnnulledException(string message) : base(message)
        {
        }
        public PurchaseOrderAnnulledException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PurchaseOrderAnnulledException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
