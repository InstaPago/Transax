using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class DeclarationNotFoundException : ITException
    {
        #region Constructors

        public DeclarationNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }
        public DeclarationNotFoundException(string message) : base(message)
        {
        }
        public DeclarationNotFoundException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public DeclarationNotFoundException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
