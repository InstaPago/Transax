using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class InactiveUserException : ITException
    {
        #region Constructors

        public InactiveUserException(string message, Exception ex) : base(message, ex)
        {
        }
        public InactiveUserException(string message) : base(message)
        {
        }
        public InactiveUserException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public InactiveUserException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
