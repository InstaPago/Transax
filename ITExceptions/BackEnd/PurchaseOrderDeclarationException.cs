using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class PurchaseOrderDeclarationException : ITException
    {
        #region Constructors

        public PurchaseOrderDeclarationException(string message, Exception ex) : base(message, ex)
        {
        }
        public PurchaseOrderDeclarationException(string message) : base(message)
        {
        }
        public PurchaseOrderDeclarationException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public PurchaseOrderDeclarationException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
