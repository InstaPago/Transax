using System;

namespace InstaTransfer.ITExceptions.BackEnd
{ 
    public class DeclarationAnnulledException : ITException
    {
        #region Constructors

        public DeclarationAnnulledException(string message, Exception ex) : base(message, ex)
        {
        }
        public DeclarationAnnulledException(string message) : base(message)
        {
        }
        public DeclarationAnnulledException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public DeclarationAnnulledException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
