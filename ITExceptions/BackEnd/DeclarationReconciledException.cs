using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class DeclarationReconciledException : ITException
    {
        #region Constructors

        public DeclarationReconciledException(string message, Exception ex) : base(message, ex)
        {
        }
        public DeclarationReconciledException(string message) : base(message)
        {
        }
        public DeclarationReconciledException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public DeclarationReconciledException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
